package main

type LoadBalancer struct {
	catalogCount   int
	inventoryCount int
}

func NewLoadBalancer() *LoadBalancer {
	return &LoadBalancer{
		catalogCount:   0,
		inventoryCount: 0,
	}
}

func (lb *LoadBalancer) GetNext(registry map[string][]string, serviceType Service) string {
	var next string

	if serviceType == Inventory {
		if len(registry[string(Inventory)]) == 0 {
			return ""
		}

		if lb.inventoryCount >= len(registry[string(Inventory)]) {
			lb.inventoryCount = 0
		}

		next = registry[string(Inventory)][lb.inventoryCount]
		lb.inventoryCount = lb.inventoryCount + 1
	} else {
		if len(registry[string(Catalog)]) == 0 {
			return ""
		}

		if lb.catalogCount >= len(registry[string(Catalog)]) {
			lb.catalogCount = 0
		}

		next = registry[string(Catalog)][lb.catalogCount]
		lb.catalogCount = lb.catalogCount + 1
	}

	return next
}
