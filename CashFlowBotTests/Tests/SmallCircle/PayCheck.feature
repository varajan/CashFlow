Feature: Paycheck

Scenario: I got payed every month
	Given I am 'Cyrus Humphrey' user
		And I play as 'Teacher'
	When I get a paycheck
	Then I have $2,620 in cash
	And My history data is following:
"""
• Get $1,110
"""

Scenario: I can undo Paycheck action
	Given I am 'Ezekiel Burton' user
		And I play as 'Teacher'
	When I get a paycheck
		And I get a paycheck
	But I rollback last action
	Then I have $2,620 in cash
	And My history data is following:
"""
• Get $1,110
"""
