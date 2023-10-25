FROM golang:1.21.1 as builder

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . .

RUN CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -o app .

FROM alpine:3.14

ENV GIN_MODE=release

COPY --from=builder /app/app /app/app

EXPOSE 8081

CMD ["/app/app"]