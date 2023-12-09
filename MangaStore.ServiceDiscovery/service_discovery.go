package main

import (
	"encoding/json"
	"io"
	"net/http"

	"github.com/gin-gonic/gin"
)

type Service string

type Registry struct {
	Inventory []string `json:"Inventory"`
	Catalog   []string `json:"Catalog"`
}

const (
	Catalog   Service = "Catalog"
	Inventory Service = "Inventory"
)

type ServiceDiscovery struct {
	registry Registry
	router   *gin.Engine
}

type ServiceRegistration struct {
	Url string
}

func NewServiceDiscovery() *ServiceDiscovery {
	registry := Registry{}
	registry.Inventory = []string{}
	registry.Catalog = []string{}

	return &ServiceDiscovery{
		registry: registry,
		router:   gin.Default(),
	}
}

func (sd *ServiceDiscovery) Run() { 
	sd.router.GET("services", sd.getServicesHandler)
	sd.router.POST("catalog/register", sd.registerCatalogHandler)
	sd.router.POST("inventory/register", sd.registerInventoryHandler)

	sd.router.Run(":8081")
}

func (sd *ServiceDiscovery) getServicesHandler(c *gin.Context) {
	err := json.NewEncoder(c.Writer).Encode(sd.registry)

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error encoding response body"})
	}

	c.Writer.WriteHeader(200)
	c.Writer.Header().Set("Content-Type", "application/json")
}

func (sd *ServiceDiscovery) registerCatalogHandler(c *gin.Context) {
	sd.baseRegisterHandler(c, Catalog)
}

func (sd *ServiceDiscovery) registerInventoryHandler(c *gin.Context) {
	sd.baseRegisterHandler(c, Inventory)
}

func (sd *ServiceDiscovery) baseRegisterHandler(c *gin.Context, serviceType Service) {
	reqBody, err := io.ReadAll(c.Request.Body)

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error reading request body"})
		return
	}

	var obj ServiceRegistration

	if err = json.Unmarshal(reqBody, &obj); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error parsing request body"})
        return
    }

	sd.register(serviceType, obj.Url)
}

func (sd *ServiceDiscovery) register(serviceType Service, url string) {
	if serviceType == Inventory {
		sd.registry.Inventory = removeDuplicates(append(sd.registry.Inventory, url))
	} else {
		sd.registry.Catalog = removeDuplicates(append(sd.registry.Catalog, url))
	}
}

func removeDuplicates(arr []string) []string {
    encountered := map[string]bool{}    
    result := []string{}                

    for _, v := range arr {
        if encountered[v] == false {
            encountered[v] = true     
            result = append(result, v) 
        }
    }

    return result
}