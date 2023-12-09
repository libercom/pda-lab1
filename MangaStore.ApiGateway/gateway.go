package main

import (
	"bytes"
	"context"
	"encoding/json"
	"io"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

type ServiceRegistration struct {
	Url string
}

const (
	Catalog   Service = "Catalog"
	Inventory Service = "Inventory"
)

type ApiGateway struct {
	lb *LoadBalancer
	cb *CircuitBreaker
	cache *Cache
	router *gin.Engine
	timeoutTime time.Duration
	maxReroutes int64
	taskCh chan struct{}
}

func NewApiGateway(timeoutTime, maxConcurrentTasks, maxReroutes int64) *ApiGateway {
	return &ApiGateway{
		lb: NewLoadBalancer(),
		cb: NewCircuitBreaker(),
		cache: NewCache(),
		router: gin.Default(),
		timeoutTime: time.Duration(timeoutTime),
		maxReroutes: maxReroutes,
		taskCh: make(chan struct{}, maxConcurrentTasks),
	}
}

func (g *ApiGateway) Run() {
	reg := prometheus.NewRegistry()
	reg.MustRegister(httpRequestsTotal)
	reg.MustRegister(customCounter)
	customCounter.WithLabelValues("value1", "value2").Inc()

	// Middleware
	g.router.Use(func(c *gin.Context) {
        httpRequestsTotal.WithLabelValues(c.Request.Method).Inc()
        c.Next()
    })

	// Handler for metrics
	g.router.GET("/metrics", gin.WrapH(promhttp.HandlerFor(reg, promhttp.HandlerOpts{Registry: reg})))

	// Status endpoint
	g.router.GET("status", func(ctx *gin.Context) {
		ctx.Writer.WriteHeader(200)
	})

	// Catalog Routes
	g.router.GET("catalog", g.catalogGetAllHandler)
	g.router.GET("catalog/:id", g.catalogGetByIdHandler)
	g.router.POST("catalog", g.catalogAddMangaHandler)
	g.router.DELETE("catalog/:id", g.catalogDeleteHandler)

	// Inventory Routes
	g.router.GET("inventory/stocks/:mangaId", g.inventoryGetStocksByMangaIdHandler)
	g.router.PUT("inventory/stocks", g.inventoryUpdateStockHandler)
	g.router.POST("inventory/stocks", g.inventoryAddStockHandler)

	g.router.GET("inventory/locations", g.inventoryGetAllLocationsHandler)
	g.router.POST("inventory/locations", g.inventoryAddLocationHandler)

	g.router.GET("inventory/orders", g.inventoryGetAllOrdersHandler)
	g.router.POST("inventory/orders", g.inventoryAddOrderHandler)

	// Run application
	g.router.Run(":8080")
}

// Catalog Handlers
func (g *ApiGateway) catalogGetAllHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "GET", "mangas", "", 0, nil)
}

func (g *ApiGateway) catalogGetByIdHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "GET", "mangas", "id", 0, nil)
}

func (g *ApiGateway) catalogAddMangaHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "POST", "mangas", "", 0, nil)
}

func (g *ApiGateway) catalogDeleteHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "DELETE", "mangas", "id", 0, nil)
}

// Inventory Handlers
func (g *ApiGateway) inventoryGetStocksByMangaIdHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "GET", "stocks", "mangaId", 0, nil)
}

func (g *ApiGateway) inventoryAddStockHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "POST", "stocks", "", 0, nil)
}

func (g *ApiGateway) inventoryUpdateStockHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "PUT", "stocks", "", 0, nil)
}

func (g *ApiGateway) inventoryGetAllLocationsHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "GET", "locations", "", 0, nil)
}

func (g *ApiGateway) inventoryAddLocationHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "POST", "locations", "", 0, nil)
}

func (g *ApiGateway) inventoryGetAllOrdersHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "GET", "orders", "", 0, nil)
}

func (g *ApiGateway) inventoryAddOrderHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "POST", "orders", "", 0, nil)
}

func (g *ApiGateway) baseHandler(c *gin.Context, serviceType Service, method, urlSuffix, param string, rerouteCount int64, reqBody []byte) {
	if rerouteCount >= g.maxReroutes {
		rerouteCount = 0
		log.Println("MAX REROUTES EXCEEDED")
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Something went wrong"})
		return
	}

	// handle get registry from Service Discovery
	registry := make(map[string][]string)
	remoteRegistryUrl := os.Getenv("REMOTE_REGISTRY_URL")
	res, err := http.Get(remoteRegistryUrl)

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error requesting service discovery registry"})
		return
	}

	defer res.Body.Close()

	body, err := io.ReadAll(res.Body)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error reading response body"})
		return
	}

	err = json.Unmarshal(body, &registry)

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err})
		return
	}

	log.Println(registry)

	// handle load balancing
	url := g.lb.GetNext(registry, serviceType)

	if url == "" {
		c.JSON(http.StatusBadGateway, gin.H{"error": "Bad gateway"})
		return
	}
	
	path := "/" + urlSuffix
	
	if param != "" {
		paramValue := c.Param(param)
		path = path + "/" + paramValue
	}
	
	url = url + path

	isHealthy := g.cb.IsHealthy(url)

	if !isHealthy {
		log.Println("REROUTE:")
		g.baseHandler(c, serviceType, method, urlSuffix, param, rerouteCount + 1, reqBody)
		return
	}

	// handle task concurrency limit
	select {
	case g.taskCh <- struct{}{}:
		defer func() {
			<-g.taskCh 
		}()
	default:
		c.JSON(http.StatusTooManyRequests, gin.H{"error": "Concurrent task limit reached"})
		return
	}
	
	// handle task timeout
	ctx, cancel := context.WithTimeout(context.Background(), g.timeoutTime * time.Second)
	defer cancel()

	// handle cache
	if serviceType == Catalog && method == "GET" {
		val, err := g.cache.client.Get(context.Background(), path).Result()

		if err == nil {
			log.Println("Returned data from cache")
			c.JSON(http.StatusOK, gin.H{"data": val})
			return
		}
	}

	// handle redirect request
	if (reqBody == nil) {
		reqBody, err = io.ReadAll(c.Request.Body)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Error reading request body"})
			return
		}
	}

	client := &http.Client{}
	req, err := http.NewRequest(method, url, bytes.NewBuffer(reqBody))

	if err != nil {
        c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        return
    }

	req.Header.Set("Content-Type", "application/json")

	req = req.WithContext(ctx)
    res, err = client.Do(req)
	
    if err != nil {
		log.Println("REROUTE:")
		g.cb.CheckService(url)
		g.baseHandler(c, serviceType, method, urlSuffix, param, rerouteCount + 1, reqBody)
		log.Println(err.Error())
        return
    }
	
    defer res.Body.Close()

	resBody, err := io.ReadAll(res.Body)

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error reading response body"})
		return
	}

	// handle write to cache
	if serviceType == Catalog && method == "GET" {
		err := g.cache.client.Set(context.Background(), path, resBody, 5 * time.Minute).Err()

		if err != nil {
			log.Println("Failed to write to cache", err)
		}
	}

	log.Println(string(resBody))

	c.JSON(res.StatusCode, gin.H{"data": string(resBody)})
	rerouteCount = 0
}