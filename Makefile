.PHONY: proto

proto:
	@command -v buf >/dev/null 2>&1 || { echo "buf CLI not found. Please install: brew install bufbuild/buf/buf"; exit 1; }
	rm -rf proto
	buf export buf.build/hobom/hobom-buf-proto --output proto
