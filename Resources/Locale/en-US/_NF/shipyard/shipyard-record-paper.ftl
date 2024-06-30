shipyard-record-paper-name = {$vessel_name} {$time}
# shipyard-record-paper-content =  [head = 1] Vessel ownership record [/head]
#      Designation: [head = 2]{$vessel_name}[/head]
#      Owneer: [head = 2]Captain {$vessel_owner}[/head]
#      Time of purchase: [head = 2]{$time}[/head]
#      Shuttle has been purchased from Nanotrasen as per New Frontiers program contract
#      Captain is held responsible to uphold SpaceLaw as well as betterment of their crew.
#
#
#      Additional notes:

shipyard-record-paper-content =
      {"["}color=blue]◥█▄  █  ®[/color]                                                               [color= #009100][italic]Frontier Automated[/italic][/color]
      {"["}color=blue]   █  ▀█◣[/color]                                                          [color= #009100][italic]Vessel Reporting System[/italic][/color]
      __________________________________________________________________
      
      {"["}head=2]               Ship Deployment Report[/head]
      __________________________________________________________________
      
      {"["}bold]Ship ID:[/bold] {$vessel_name}
      
      {"["}bold]Time Deployed:[/bold] {$time}
      
      {"["}bold]Captain Name:[/bold] {$vessel_owner_name}
      {""}                [italic]Species:[/italic] {$vessel_owner_species}
      {""}                [italic]Gender:[/italic] {$vessel_owner_gender}
      {""}                   [italic]Age:[/italic] {$vessel_owner_age}
      {""}
      {""}                [italic]Fingerprints:[/italic] {$vessel_owner_fingerprints}
      {""}                [italic]DNA:[/italic] {$vessel_owner_dna}
      __________________________________________________________________
      
      {"["}color=grey][italic]         This automated report is accurate at the time of reciept.[/italic][/color]
      
      {"["}color=grey][italic] The above information may change during the course of the shift.[/italic][/color]
      __________________________________________________________________
