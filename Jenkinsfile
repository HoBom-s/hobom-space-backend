@Library('hobom-shared-lib') _
hobomPipeline(
  serviceName:    'dev-hobom-space-backend',
  hostPort:       '8083',
  containerPort:  '8080',
  memory:         '512m',
  cpus:           '0.5',
  envPath:        '/etc/hobom-dev/dev-hobom-space-backend/.env',
  addHost:        true,
  submodules:     false,
  extraPorts:     ['50052:50052'],
  extraVolumes:   ['/home/infra-admin/certs:/etc/grpc-tls:ro'],
  smokeCheckPath: '/health/live'
)
