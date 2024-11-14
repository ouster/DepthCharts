# DepthCharts

# .NET core 8

# Running thru Rider/Visual Studio/dotnet cli

This app opens a swagger page with CRUD endpoints to query and manipulate a depth chart

I've added 2 scraping endpoints to grab a list of teams and scrape current depth charts for NFL

Only NFL is supported via the ourlads domain

Other sports can be added provided you add a custom scraper for the given sport

## Testing

Scraping a given sport/team via swagger will populate the depthchart for that team and you can query it using fullDepthChart

Note I've not tested all the teams in ourlads 

e.g. HOU team has bad data, the scraper does cope with this by repairing the data and logging a warning

## future

A more production ready implementation would probably persist this somewhere say redis 

I've added a docker file (and there is a docker-compose in the git history, may bring it back)