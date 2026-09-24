# AI DM Toolkit

A Blazor Server application that helps Dungeon Masters generate 
tabletop RPG content using the OpenAI API.

## Features

- **NPC Generator** - Generate detailed D&D characters with 
  personality, secrets, combat style, and plot hooks
- **Town Generator** - Create immersive settlements with notable 
  locations, lore, quests, and NPCs

## Tech Stack

- Blazor Server (.NET 8)
- OpenAI API (gpt-4.1-mini)
- JSON Schema-constrained structured outputs mapped to 
  strongly typed C# models
- Bootstrap + custom dark theme UI

## How to Run

1. Clone the repository
2. Add your OpenAI API key to User Secrets:
```bash
   dotnet user-secrets set "OpenAI:ApiKey" "your-key-here"
```
3. Run the project:
```bash
   dotnet run
```

## Status

Proof of concept - core generation features working. 
Backend API integration planned.
