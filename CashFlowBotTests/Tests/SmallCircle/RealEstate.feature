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

Scenario: I can sell real estate
	Given I am 'Jacques Hamilton' user
		And I play as 'Pilot'
		And I get $100,000 in cash
	When I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |              160 |
		| Small       | 3/2    |  65,000 |         3,000 |             -100 |
		| Big         | 3/2    |  90,000 |         7,000 |              400 |
		| Big         | 2-plex | 125,000 |         9,000 |              500 |
		| Big         | 8-plex | 150,000 |        10,000 |            1,600 |
		But I sell 2/1 for $100,000
		And I sell 8-plex for $45,000 each
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $334,000
*Salary:* $9,500
*Income:* $800
*Expenses:* $6,900
*Cash Flow*: $3,400

*Assets:*
• *3/2* - Price: $65,000, Mortgage: $62,000, Cash Flow: -$100
• *3/2* - Price: $90,000, Mortgage: $83,000, Cash Flow: $400
• *2-plex* - Price: $125,000, Mortgage: $116,000, Cash Flow: $500

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $100,000
• Buy Real Estate. *2/1* - Price: $60,000, Mortgage: $58,000, Cash Flow: $160
• Buy Real Estate. *3/2* - Price: $65,000, Mortgage: $62,000, Cash Flow: -$100
• Buy Real Estate. *3/2* - Price: $90,000, Mortgage: $83,000, Cash Flow: $400
• Buy Real Estate. *2-plex* - Price: $125,000, Mortgage: $116,000, Cash Flow: $500
• Buy Real Estate. *8-plex* - Price: $150,000, Mortgage: $140,000, Cash Flow: $1,600
• Sell Real Estate. *2/1* - Price: $100,000
• Sell Real Estate. *8-plex* - Price: $45,000
"""

Scenario: I can rollback buy transaction
	Given I am 'Lilli Atkins' user
		And I play as 'Pilot'
		And I get $10,000 in cash
	When I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |              160 |
		| Big         | 8-plex | 150,000 |        10,000 |            1,600 |
		But I rollback last action
	Then I have $11,000 in cash
		And My passive income is $160
		And My assets are:
"""
• *2/1* - Price: $60,000, Mortgage: $58,000, Cash Flow: $160
"""

Scenario: I can rollback sell transaction
	Given I am 'Zaina Ramirez' user
		And I play as 'Pilot'
		And I get $10,000 in cash
	When I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |              160 |
		| Big         | 8-plex | 150,000 |        10,000 |            1,600 |
		And I sell 2/1 for $100,000
		But I rollback last action
	Then I have $1,000 in cash
		And My passive income is $1,760
		And My assets are:
"""
• *2/1* - Price: $60,000, Mortgage: $58,000, Cash Flow: $160
• *8-plex* - Price: $150,000, Mortgage: $140,000, Cash Flow: $1,600
"""
