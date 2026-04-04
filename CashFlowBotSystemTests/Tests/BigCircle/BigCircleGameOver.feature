Feature: Big Circle GameOver

Background:
	Given Few players:
	| Name          | Profession |
	| Marina Norris | Doctor     |
	| Arman Ortiz   | Pilot      |
	| Elmer Chang   | Lawyer     |
	| Neil Porter   | Teacher    |
	| Renee Rocha   | Secretary  |

	When Marina Norris gets 2 kids
	When Arman Ortiz get $5,000 as a credit

	When Elmer Chang starts the Computer Programs company with $2000
		And Elmer Chang increases the cash flow of my small business by $7,000
		And Elmer Chang goes to the Big Circle

	When Neil Porter buys businesses:
		| Title   | Price   | First Payment | Monthly Cashflow |
		| Passage | 100,000 |         1,000 |            3,000 |
		And Neil Porter goes to the Big Circle

	But Renee Rocha buys a boat


Scenario: I can win the game
	When Elmer Chang buys big businesses:
	| Title              | Price   | Cashflow |
	| Gold mine          | 100,000 |   25,000 |
	| 60-plex            | 100,000 |   25,000 |
	Then Elmer Chang recieved notification: 'You are the winner!'
	And All users, except Elmer Chang recieve notification: Elmer Chang is the winner!

Scenario: I can stop the game after my victory
	When Elmer Chang buys big businesses:
	| Title              | Price   | Cashflow |
	| Gold mine          | 100,000 |   25,000 |
	| 60-plex            | 100,000 |   25,000 |
	And Elmer Chang say 'Stop game'
	And Elmer Chang say 'yes'
	Then The game is restarted for Elmer Chang

Scenario: I can stop the game
	When Elmer Chang decides to stop the game
		And Elmer Chang say 'yes'
	Then The game is restarted for Elmer Chang

Scenario: I choose not to stop the game
	When Elmer Chang decides to stop the game
		And Elmer Chang say 'cancel'
	Then The game is continued for Elmer Chang
