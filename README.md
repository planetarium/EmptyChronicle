# EmptyChronicle

EmptyChonicle (or 0c) is simple node for NineChronicles providing http api about block, state, etc.

## Architecture

I recommend reading this section and follow this guide if you want to contribute to this proejct.

```
EmptyChronicle
├── Domain
│   ├── Model
│   │   ├── Entity
│   │   └── Repository (** Interface)
│   └── Service (** Domain Service)
├── Application
│   ├── Application (** Application Service)
│   └── ApplicationInjectionExtensions.cs
│       You should add applications as dependancy into AIE.cs
├── Controller
│   └── Controller (** Application Service)
├── Infrastructure
│   └── Repository (** Implementations)
└── Program.cs
```

I Recommend to follow these steps to add new API
1. Create or modify domain objects with only domain logic.
    - Entity: Basically, entity should have all domain logic.
    - Repository: Interface (definition of method) to persistence entity.
    - Domain Service: Simply, it's collection of static method of entity but we seperated it into another class to inject repository.
2. With domain objects, write usecase in application (service).
    - Application can inject repository so inquire entity that you want with repository, and invoke some domain logic, finally persist domain with repository if you need.
    - It's an usecase so you can test it with repository implemented with mock
3. Implement repository interface.
4. Write controller to invoke application. (Mostly HTTP)
