Feature: RealEstate

Scenario: I can buy real estate
	Given I am 'Dewi Walls' user
		And I play as 'Pilot'
		And I get $100,000 in cash
	When I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |              160 |
		| Small       | 3/2    |  65,000 |         3,000 |             -100 |
		| Big         | 3/2    |  90,000 |         7,000 |              400 |
		| Big         | 2-plex | 125,000 |         9,000 |              500 |
		| Big         | 8-plex | 150,000 |        10,000 |            1,600 |
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $72,000
*Salary:* $9,500
*Income:* $2,560
*Expenses:* $6,900
*Cash Flow*: $5,160

*Assets:*
• *2/1* - Price: $60,000, Mortgage: $58,000, Cash Flow: $160
• *3/2* - Price: $65,000, Mortgage: $62,000, Cash Flow: -$100
• *3/2* - Price: $90,000, Mortgage: $83,000, Cash Flow: $400
• *2-plex* - Price: $125,000, Mortgage: $116,000, Cash Flow: $500
• *8-plex* - Price: $150,000, Mortgage: $140,000, Cash Flow: $1,600

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Other Payments:* $2,210
"""
