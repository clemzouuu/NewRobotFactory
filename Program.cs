﻿// Projet complet en C# - Usine de robots avec Design Patterns (phase 2+3+4)

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

// --- TEMPLATE ROBOT ---
class RobotTemplate {
    public string Name;
    public Category RobotCategory;
    public string Core, Generator, Arms, Legs, System;

    public RobotTemplate(string name, Category cat, string core, string system, string gen, string arms, string legs) {
        Name = name; 
        RobotCategory = cat;
        Core = core;
        System = system;
        Generator = gen;
        Arms = arms;
        Legs = legs;
    }
}

class RobotFactory {
    private Dictionary<string, RobotTemplate> templates = new();
    public void Add(RobotTemplate t) {
        if (ValidateTemplate(t)) templates[t.Name] = t;
        else Console.WriteLine($"ERROR Invalid template {t.Name}");
    }
    public RobotTemplate Get(string name) => templates.ContainsKey(name) ? templates[name] : throw new Exception($"Unknown robot: {name}");
    public bool Exists(string name) => templates.ContainsKey(name);
    private bool ValidateTemplate(RobotTemplate t)
    {
        var allowedParts = t.RobotCategory switch {
            Category.D => new[] { Category.D, Category.G, Category.I },
            Category.I => new[] { Category.I, Category.G },
            Category.M => new[] { Category.M, Category.I },
            _ => Array.Empty<Category>()
        };

        var allowedSystem = t.RobotCategory switch {
            Category.D => new[] { Category.D, Category.G, Category.I },
            Category.I => new[] { Category.I, Category.G },
            Category.M => new[] { Category.M, Category.G },
            _ => Array.Empty<Category>()
        };

        // Valide les pièces (hors système)
        var parts = new[] { t.Core, t.Generator, t.Arms, t.Legs };
        foreach (var part in parts)
        {
            if (!PartDatabase.Categories.TryGetValue(part, out var category) || !allowedParts.Contains(category))
                return false;
        }

        // Valide le système séparément
        if (!PartDatabase.Categories.TryGetValue(t.System, out var systemCategory) || !allowedSystem.Contains(systemCategory))
            return false;

        return true;
    }

    public IEnumerable<string> List() => templates.Keys;
}

// --- BASE DE DONNÉES ---
static class PartDatabase {
    public static Dictionary<string, Category> Categories = new();
    public static void Init() {
        // Cores
        Categories["Core_CM1"] = Category.M;
        Categories["Core_CD1"] = Category.D;
        Categories["Core_CI1"] = Category.I;
        // Generators
        Categories["Generator_GM1"] = Category.M;
        Categories["Generator_GD1"] = Category.D;
        Categories["Generator_GI1"] = Category.I;
        // Arms
        Categories["Arms_AM1"] = Category.M;
        Categories["Arms_AD1"] = Category.D;
        Categories["Arms_AI1"] = Category.I;
        // Legs
        Categories["Legs_LM1"] = Category.M;
        Categories["Legs_LD1"] = Category.D;
        Categories["Legs_LI1"] = Category.I;
        // Systems
        Categories["System_SB1"] = Category.G;
        Categories["System_SM1"] = Category.M;
        Categories["System_SD1"] = Category.D;
        Categories["System_SI1"] = Category.I;
    }
}

class Stock
{
    public Dictionary<string, int> Parts = new();
    public Dictionary<string, int> Robots = new();

    public void Add(string name, int qty, bool robot = false)
    {
        var dict = robot ? Robots : Parts;
        if (!dict.ContainsKey(name)) dict[name] = 0;
        dict[name] += qty;
    }

    public bool Consume(string name, int qty)
    {
        if (!Parts.ContainsKey(name) || Parts[name] < qty) return false;
        Parts[name] -= qty;
        return true;
    }

    public void Show()
    {
        foreach (var r in Robots) Console.WriteLine($"{r.Value} {r.Key}");
        foreach (var p in Parts) Console.WriteLine($"{p.Value} {p.Key}");
    }
}

// --- COMMANDES ---
    interface ICommand
    {
        void Execute(string[] args);
    }

    class StockCommand : ICommand
    {
        Stock stock;
        public StockCommand(Stock s) => stock = s;
        public void Execute(string[] args) => stock.Show();
    }

    class NeededCommand : ICommand
    {
        RobotFactory factory;
        public NeededCommand(RobotFactory f) => factory = f;

        public void Execute(string[] args)
        {
            var map = ParseArgs(args);
            var total = new Dictionary<string, int>();
            foreach (var (k, v) in map)
            {
                var t = factory.Get(k);
                var list = new[] { t.Core, t.Generator, t.Arms, t.Legs, t.System };
                Console.WriteLine($"{v} {k}:");
                foreach (var p in list)
                {
                    Console.WriteLine($"{v} {p}");
                    if (!total.ContainsKey(p)) total[p] = 0;
                    total[p] += v;
                }
            }

            Console.WriteLine("Total:");
            foreach (var (k, v) in total) Console.WriteLine($"{v} {k}");
        }

        Dictionary<string, int> ParseArgs(string[] args) => string.Join(" ", args).Split(',')
            .Select(x => x.Trim().Split(' ')).GroupBy(p => p[1])
            .ToDictionary(g => g.Key, g => g.Sum(p => int.Parse(p[0])));
    }

    class InstructionCommand : ICommand
    {
        RobotFactory factory;
        public InstructionCommand(RobotFactory f) => factory = f;

        public void Execute(string[] args)
        {
            var map = ParseArgs(args);
            foreach (var (k, v) in map)
            {
                var t = factory.Get(k);
                for (int i = 0; i < v; i++)
                {
                    Console.WriteLine($"PRODUCING {k}");
                    Console.WriteLine($"GET_OUT_STOCK 1 {t.Core}");
                    Console.WriteLine($"GET_OUT_STOCK 1 {t.Generator}");
                    Console.WriteLine($"GET_OUT_STOCK 1 {t.Arms}");
                    Console.WriteLine($"GET_OUT_STOCK 1 {t.Legs}");
                    Console.WriteLine($"INSTALL {t.System} {t.Core}");
                    Console.WriteLine($"ASSEMBLE TMP1 {t.Core} {t.Generator}");
                    Console.WriteLine($"ASSEMBLE TMP2 TMP1 {t.Arms}");
                    Console.WriteLine($"ASSEMBLE TMP3 TMP2 {t.Legs}");
                    Console.WriteLine($"FINISHED {k}");
                }
            }
        }

        Dictionary<string, int> ParseArgs(string[] args) => string.Join(" ", args).Split(',')
            .Select(x => x.Trim().Split(' ')).GroupBy(p => p[1])
            .ToDictionary(g => g.Key, g => g.Sum(p => int.Parse(p[0])));
    }

    class VerifyCommand : ICommand
    {
        RobotFactory factory;
        Stock stock;
        public VerifyCommand(RobotFactory f, Stock s) => (factory, stock) = (f, s);

        public void Execute(string[] args)
        {
            try
            {
                var map = string.Join(" ", args).Split(',').Select(x => x.Trim().Split(' ')).GroupBy(p => p[1])
                    .ToDictionary(g => g.Key, g => g.Sum(p => int.Parse(p[0])));
                foreach (var r in map.Keys)
                    if (!factory.Exists(r))
                        throw new Exception($"{r} is not a recognized robot");
                var total = new Dictionary<string, int>();
                foreach (var (k, v) in map)
                {
                    var t = factory.Get(k);
                    foreach (var p in new[] { t.Core, t.Generator, t.Arms, t.Legs, t.System })
                    {
                        if (!total.ContainsKey(p)) total[p] = 0;
                        total[p] += v;
                    }
                }

                foreach (var (p, qty) in total)
                    if (!stock.Parts.ContainsKey(p) || stock.Parts[p] < qty)
                        throw new Exception("UNAVAILABLE");
                Console.WriteLine("AVAILABLE");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR " + ex.Message);
            }
        }
    }

    class ProduceCommand : ICommand
    {
        RobotFactory factory;
        Stock stock;
        public ProduceCommand(RobotFactory f, Stock s) => (factory, stock) = (f, s);

        public void Execute(string[] args)
        {
            try
            {
                var map = string.Join(" ", args).Split(',').Select(x => x.Trim().Split(' ')).GroupBy(p => p[1])
                    .ToDictionary(g => g.Key, g => g.Sum(p => int.Parse(p[0])));
                var total = new Dictionary<string, int>();
                foreach (var (k, v) in map)
                {
                    var t = factory.Get(k);
                    foreach (var p in new[] { t.Core, t.Generator, t.Arms, t.Legs, t.System })
                    {
                        if (!total.ContainsKey(p)) total[p] = 0;
                        total[p] += v;
                    }
                }

                foreach (var (p, qty) in total)
                    if (!stock.Consume(p, qty))
                        throw new Exception("UNAVAILABLE");
                foreach (var (k, v) in map) stock.Add(k, v, true);
                Console.WriteLine("STOCK_UPDATED");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR " + ex.Message);
            }
        }
    }

    class AddTemplateCommand : ICommand
    {
        RobotFactory factory;
        public AddTemplateCommand(RobotFactory f) => factory = f;

        public void Execute(string[] args)
        {
            try
            {
                var parts = string.Join(" ", args).Split(',').Select(x => x.Trim()).ToArray();
                if (parts.Length < 6) throw new Exception("Invalid template format");
                var name = parts[0];
                var core = parts[1];
                var system = parts[2];
                var generator = parts[3];
                var arms = parts[4];
                var legs = parts[5];
                var cat = PartDatabase.Categories.ContainsKey(core)
                    ? PartDatabase.Categories[core]
                    : throw new Exception("Unknown core category");
                factory.Add(new RobotTemplate(name, cat, core, system, generator, arms, legs));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR " + ex.Message);
            }
        }
    }

// --- APP ---
    class App
    {
        Dictionary<string, ICommand> commands = new();
        public void Register(string cmd, ICommand impl) => commands[cmd] = impl;

        public void Run()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var split = line.Split(' ', 2);
                var cmd = split[0].ToUpper();
                var args = split.Length > 1 ? split[1].Split(' ') : new string[0];
                if (commands.ContainsKey(cmd)) commands[cmd].Execute(args);
                else Console.WriteLine("ERROR Unknown command");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            PartDatabase.Init();
            

            var stock = new Stock();
            
            // Stock initial pour test
            stock.Add("Core_CM1", 10);
            stock.Add("Generator_GM1", 10);
            stock.Add("Arms_AM1", 10);
            stock.Add("Legs_LM1", 10);
            stock.Add("System_SB1", 10);

            stock.Add("Core_CD1", 5);
            stock.Add("Generator_GD1", 5);
            stock.Add("Arms_AD1", 5);
            stock.Add("Legs_LD1", 5);

            stock.Add("Core_CI1", 3);
            stock.Add("Generator_GI1", 3);
            stock.Add("Arms_AI1", 3);
            stock.Add("Legs_LI1", 3);

            var factory = new RobotFactory();

            factory.Add(new RobotTemplate("XM-1", Category.M, "Core_CM1", "System_SB1", "Generator_GM1", "Arms_AM1",
                "Legs_LM1"));
            factory.Add(new RobotTemplate("RD-1", Category.D, "Core_CD1", "System_SB1", "Generator_GD1", "Arms_AD1",
                "Legs_LD1"));
            factory.Add(new RobotTemplate("WI-1", Category.I, "Core_CI1", "System_SB1", "Generator_GI1", "Arms_AI1",
                "Legs_LI1"));

            var app = new App();
            app.Register("STOCKS", new StockCommand(stock));
            app.Register("NEEDED_STOCKS", new NeededCommand(factory));
            app.Register("INSTRUCTIONS", new InstructionCommand(factory));
            app.Register("VERIFY", new VerifyCommand(factory, stock));
            app.Register("PRODUCE", new ProduceCommand(factory, stock));
            app.Register("ADD_TEMPLATE", new AddTemplateCommand(factory));

            app.Run();
        }
    }

