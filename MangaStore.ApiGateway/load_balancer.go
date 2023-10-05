package main

type LoadBalancer struct {
	count int
}

func NewLoadBalancer() *LoadBalancer {
	return &LoadBalancer{
		count: 0,
	}
}

func (lb *LoadBalancer) GetNext(registry Registry, serviceType Service) string {
	if len(registry[serviceType]) == 0 {
		return ""
	}

	if lb.count >= len(registry[serviceType]) {
		lb.count = 0
	}

	next := registry[serviceType][lb.count]
	lb.count = lb.count + 1

	return next
}
