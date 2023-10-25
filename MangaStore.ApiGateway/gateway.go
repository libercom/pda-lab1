package main

import (
	"bytes"
	"context"
	"encoding/json"
	"io"
	"log"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
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
	cache *Cache
	router *gin.Engine
	timeoutTime time.Duration
	taskCh chan struct{}
}

func NewApiGateway(timeoutTime, maxConcurrentTasks int64) *ApiGateway {
	return &ApiGateway{
		lb: NewLoadBalancer(),
		cache: NewCache(),
		router: gin.Default(),
		timeoutTime: time.Duration(timeoutTime),
		taskCh: make(chan struct{}, maxConcurrentTasks),
	}
}

func (g *ApiGateway) Run() {
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
	g.router.POST("inventory/locations", g.inventoryGetAllLocationsHandler)

	g.router.GET("inventory/orders", g.inventoryGetAllOrdersHandler)
	g.router.POST("inventory/orders", g.inventoryAddOrderHandler)

	// Run application
	g.router.Run(":8080")
}

// Catalog Handlers
func (g *ApiGateway) catalogGetAllHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "GET", "mangas", "")
}

func (g *ApiGateway) catalogGetByIdHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "GET", "mangas", "id")
}

func (g *ApiGateway) catalogAddMangaHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "POST", "mangas", "")
}

func (g *ApiGateway) catalogDeleteHandler(c *gin.Context) {
	g.baseHandler(c, Catalog, "DELETE", "mangas", "id")
}

// Inventory Handlers
func (g *ApiGateway) inventoryGetStocksByMangaIdHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "GET", "stocks", "mangaId")
}

func (g *ApiGateway) inventoryAddStockHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "POST", "stocks", "")
}

func (g *ApiGateway) inventoryUpdateStockHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "PUT", "stocks", "")
}

func (g *ApiGateway) inventoryGetAllLocationsHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "GET", "locations", "")
}

func (g *ApiGateway) inventoryAddLocationHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "POST", "locations", "")
}

func (g *ApiGateway) inventoryGetAllOrdersHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "GET", "orders", "")
}

func (g *ApiGateway) inventoryAddOrderHandler(c *gin.Context) {
	g.baseHandler(c, Inventory, "POST", "orders", "")
}

func (g *ApiGateway) baseHandler(c *gin.Context, serviceType Service, method, urlSuffix, param string) {
	// handle get registry from Service Discovery
	registry := make(map[string][]string)

	res, err := http.Get("http://localhost:8081/services")
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

	// handle load balancing
	url := g.lb.GetNext(registry, serviceType)
	
	path := "/" + urlSuffix

	if param != "" {
		paramValue := c.Param(param)
		path = path + "/" + paramValue
	}
	
	url = url + path

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
	reqBody, err := io.ReadAll(c.Request.Body)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error reading request body"})
		return
	}

	client := &http.Client{}
	req, err := http.NewRequest(method, url, bytes.NewBuffer(reqBody))

	log.Println(url)

	if err != nil {
        c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
        return
    }

	req = req.WithContext(ctx)
    res, err = client.Do(req)

	
    if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
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

	c.JSON(res.StatusCode, gin.H{"data": string(resBody)})
}