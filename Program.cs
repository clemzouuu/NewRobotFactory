// Projet complet en C# - Usine de robots avec Design Patterns (phase 2+3+4)

using System;
using System.Collections.Generic;
using System.Linq;

// --- ENUMS ET BASES ---
enum Category { G, D, I, M }

abstract class Part {
    public string Name { get; }
    public Category Category { get; }
    public Part(string name, Category category) => (Name, Category) = (name, category);
}

class SystemPart : Part {
    public SystemPart(string name, Category category) : base(name, category) { }
}

class Module : Part {
    public SystemPart InstalledSystem { get; private set; }
    public Module(string name, Category category) : base(name, category) { }
    public void Install(SystemPart system) => InstalledSystem = system;
}

// --- COMPOSANTS ---
class Core : Module { public Core(string n, Category c) : base(n, c) { } }
class Generator : Part { public Generator(string n, Category c) : base(n, c) { } }
class Arms : Part { public Arms(string n, Category c) : base(n, c) { } }
class Legs : Part { public Legs(string n, Category c) : base(n, c) { } }

