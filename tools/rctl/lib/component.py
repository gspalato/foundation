class ComponentPlatformBuildSettings:
    def __init__(self, build_on_compose: bool, build_on_kubernetes: bool, push_on_compose: bool, push_on_kubernetes: bool) -> None:
        self.build_on_compose: bool = build_on_compose
        self.build_on_kubernetes: bool = build_on_kubernetes
        self.push_on_compose: bool = push_on_compose
        self.push_on_kubernetes: bool = push_on_kubernetes

class ComponentBuildSettings:
    def __init__(self, context: str, dockerfile: str, platforms: ComponentPlatformBuildSettings) -> None:
        self.context: str = context
        self.dockerfile: str = dockerfile
        self.platforms: ComponentPlatformBuildSettings = platforms

class Component:
    types: list = ["database", "microservice", "application"]

    def __init__(self, id: str, name: str, type: str, path: str, build: ComponentBuildSettings):
        self.id = id
        self.name = name
        self.path = path
        self.build = build

        if (type not in self.types):
            raise ValueError("Invalid component type '" + type + "'.")
        
        self.type = type

    def __str__(self) -> str:
        return self.id