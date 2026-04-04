Feature: Bankruptcy Avoid

Background:
	Given I am 'Gladys Monroe' user
		And I play as 'Car mechanic'
	When I get $10,000 as a credit
		And I lost my job 7 times
	But I get a paycheck

Scenario: I can't avoid bankruptcy via Stop game
	When I say 'Stop game'
		And I say 'Cancel'
	Then My last message is: You are bankrupt. Game is over.

Scenario: I can't avoid bankruptcy via History
	When I say 'History'
		And I say 'Cancel'
	Then My last message is: You are bankrupt. Game is over.
