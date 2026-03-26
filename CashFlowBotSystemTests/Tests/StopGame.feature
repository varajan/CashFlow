Feature: StopGame

Scenario: I can stop the game
	Given I am 'Aurora Franklin' user
		And I play as 'Engineer'
	When I decide to stop the game
		And I say 'Yes'
	Then The game is restarted for me

Scenario: I choose not to stop the game
	Given I am 'Roosevelt Schmidt' user
		And I play as 'Engineer'
	When I decide to stop the game
		And I say 'Cancel'
	Then The game is continued for me
