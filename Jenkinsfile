@Library('hobom-jenkins-library') _

hobomPipeline(
    serviceName   : 'hobom-space-backend',
    hostPort      : '8083',
    containerPort : '8080',
    memory        : '512m',
    cpus          : '0.5',
    envPath       : '/home/infra-admin/env/hobom-space-backend/.env',
    addHost       : true,
    smokeCheckPath: '/health/live',
)
