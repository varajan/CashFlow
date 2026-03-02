@non-parallel
@do-cleanup

Feature: GiveMoney

Background:
	Given Few players:
	| Name            | Profession |
	| Haroon Stephens | Doctor     |
	| Ela Lynch       | Pilot      |
	| Brodie Newton   | Lawyer     |
	| Kaitlin Alvarez | Teacher    |

	When Ela Lynch get $1000 as a credit
	When Haroon Stephens get $5,000 in cash
	When Brodie Newton get $1,000 in cash

	When Kaitlin Alvarez get $2,000 in cash
		And Kaitlin Alvarez get $3,000 in cash
		And Kaitlin Alvarez get $5,000 in cash
		And Kaitlin Alvarez buys real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |            1,000 |
		| Big         | 8-plex | 150,000 |         5,000 |            1,500 |
		And Kaitlin Alvarez goes to the Big Circle

Scenario: Can give money to friend on Small Circle
	When Haroon Stephens says 'Give money'
	Then Haroon Stephens sees buttons: Ela Lynch, Brodie Newton, Bank, Cancel

Scenario: Can give money to friend
	When Haroon Stephens pays $1000 to Ela Lynch
	Then All users recieve notification: Haroon Stephens transferred $1,000 to Ela Lynch.
	And Balance by users is:
		| Name            | Balance  |
		| Haroon Stephens | $7,950   |
		| Ela Lynch       | $5,000   |
		| Brodie Newton   | $3,480   |
		| Kaitlin Alvarez | $254,510 |
	And Haroon Stephens history data is following:
"""
• Get $5,000
• Pay $1,000
"""
	And Ela Lynch history data is following:
"""
• Get credit: $1,000
• Get $1,000
"""

Scenario: Can rollback payment to a friend
	When Haroon Stephens pays $1000 to Ela Lynch
		But Haroon Stephens rollbacks last action
	Then Balance by users is:
		| Name            | Balance  |
		| Haroon Stephens | $8,950   |
		| Ela Lynch       | $5,000   |
		| Brodie Newton   | $3,480   |
		| Kaitlin Alvarez | $254,510 |
	And Haroon Stephens history data is following:
"""
• Get $5,000
"""
	And Ela Lynch history data is following:
"""
• Get credit: $1,000
• Get $1,000
"""

Scenario: Can rollback payment from a friend
	When Haroon Stephens pays $1000 to Ela Lynch
		But Ela Lynch rollbacks last action
	Then Balance by users is:
		| Name            | Balance  |
		| Haroon Stephens | $7,950   |
		| Ela Lynch       | $4,000   |
		| Brodie Newton   | $3,480   |
		| Kaitlin Alvarez | $254,510 |
	And Haroon Stephens history data is following:
"""
• Get $5,000
• Pay $1,000
"""
	And Ela Lynch history data is following:
"""
• Get credit: $1,000
"""

Scenario: Can pay money to bank
	When Haroon Stephens pays $1000 to bank
	Then All users recieve notification: Haroon Stephens transferred $1,000 to Bank.
	And Balance by users is:
		| Name            | Balance  |
		| Haroon Stephens | $7,950   |
		| Ela Lynch       | $4,000   |
		| Brodie Newton   | $3,480   |
		| Kaitlin Alvarez | $254,510 |
	And Haroon Stephens history data is following:
"""
• Get $5,000
• Pay $1,000
"""

Scenario: Can rollback payment to a bank
	When Haroon Stephens pays $1000 to bank
		But Haroon Stephens rollbacks last action
	Then Balance by users is:
		| Name            | Balance  |
		| Haroon Stephens | $8,950   |
		| Ela Lynch       | $4,000   |
		| Brodie Newton   | $3,480   |
		| Kaitlin Alvarez | $254,510 |
	And Haroon Stephens history data is following:
"""
• Get $5,000
"""
