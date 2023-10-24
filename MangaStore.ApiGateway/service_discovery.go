package main

type Service string

type Registry struct {
	Inventory []string `json:"inventory"`
	Catalog   []string `json:"catalog"`
}