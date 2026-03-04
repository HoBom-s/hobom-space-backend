@Library('hobom-shared-lib') _
hobomPipeline(
  serviceName:    'dev-hobom-space-backend',
  hostPort:       '8083',
  containerPort:  '8080',
  memory:         '512m',
  cpus:           '0.5',
  envPath:        '/etc/hobom-dev/dev-hobom-space-backend/.env',
  addHost:        true,
  smokeCheckPath: '/health/live'
)
