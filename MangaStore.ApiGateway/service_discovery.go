package main

type Service string
type Registry map[Service][]string

const (
	Catalog   Service = "Catalog"
	Inventory Service = "Inventory"
)

type ServiceDiscovery struct {
	Registry Registry
}

func NewServiceDiscovery() *ServiceDiscovery {
	return &ServiceDiscovery{
		Registry: make(map[Service][]string),
	}
}

func (sd *ServiceDiscovery) Register(serviceType Service, url string) {
	if sd.Registry[serviceType] == nil {
		sd.Registry[serviceType] = []string{}
	}

	sd.Registry[serviceType] = append(sd.Registry[serviceType], url)
}