package main

import "github.com/prometheus/client_golang/prometheus"

var (
	httpRequestsTotal = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "http_requests_total",
			Help: "Total number of HTTP requests",
		},
		[]string{"method"},
	)

	customCounter = prometheus.NewCounterVec(
        prometheus.CounterOpts{
            Name: "custom_counter",
            Help: "A custom counter metric",
        },
        []string{"label1", "label2"},
    )
)