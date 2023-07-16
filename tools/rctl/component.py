class Component:
    types: list = ["database", "microservice", "application"]

    def __init__(self, id: str, name: str, type: str, path: str):
        self.id = id
        self.name = name
        self.path = path

        if (type not in self.types):
            raise ValueError("Invalid component type '" + type + "'.")
        
        self.type = type

    def __str__(self) -> str:
        return self.id