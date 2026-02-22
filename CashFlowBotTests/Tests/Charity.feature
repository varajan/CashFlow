Feature: Charity

Scenario: I can donate to a charity
	Given I am 'Zachary Larson' user
		And I play as 'Engineer'
	When I donate to a charity
	Then I have $1,600 in cash
	And My history data is following:
"""
• Charity: $490
"""

Scenario: I can undo donate action
	Given I am 'Luis Bowman' user
		And I play as 'Engineer'
	When I donate to a charity
	But I rollback last action
	Then I have $2,090 in cash
