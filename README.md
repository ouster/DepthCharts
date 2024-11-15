# DepthCharts

# .NET core 8

# Running thru Rider/Visual Studio/dotnet cli

This app opens a swagger page with CRUD endpoints to query and manipulate a depth chart

### I've added 2 scraping endpoints to grab a list of teams and scrape current depth charts for NFL

- Only NFL is supported for scraping
- Other sports can be added provided you add a custom scraper for the given sport or other source of data

## Testing

1. Run the API - it auto opens a swagger ui page
2. Populate a depth chart using the scrape endpoint DepthChart/NFL/TB/scrape e.g. Sport: NFL, Team TB (uppercase)
3. use the other endpoints to play with the depth chart

- Scraping a given sport/team via swagger will populate the depthchart for that team and you can query it using fullDepthChart
- Note I've not tested all the teams in ourlads 
- e.g. HOU team has bad data, the scraper does cope with this by repairing the data and logging a warning

## future

A more production ready implementation would probably persist this somewhere say redis or entity framework to a data repo  
My attempt at redis may be in the git history if curious next attempt would leverage the learnings from the first attempt
- e.g. use Redis.OM or just use a DB

I've added a docker file (and there is a docker-compose in the git history, may bring it back)