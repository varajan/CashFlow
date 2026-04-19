Feature: Big Circle

Background:
	Given Few players:
	| Name               | Profession |
	| Genevieve Franklin | Doctor     |
	| Holly Bentley      | Pilot      |
	| Rory Hartman       | Lawyer     |
	| Eoin Owens         | Teacher    |
	| Deborah Case       | Secretary  |

	When Genevieve Franklin gets 2 kids
	When Holly Bentley get $5,000 as a credit

	When Rory Hartman starts the Computer Programs company with $2000
		And Rory Hartman increases the cash flow of my small business by $7,000
		And Rory Hartman goes to the Big Circle

	When Eoin Owens pays $500 to Deborah Case
	When Eoin Owens buys businesses:
		| Title   | Price   | First Payment | Monthly Cashflow |
		| Passage | 100,000 |         1,000 |            3,000 |
		And Eoin Owens goes to the Big Circle

	But Deborah Case buys a boat


Scenario: I can get my pay check
	When Eoin Owens gets a paycheck
	Then Eoin Owens has $600,010 in cash
	And Eoin Owens history data is following:
"""
• Pay $500
• Buy Business. *Passage* - Price: $100,000, Mortgage: $99,000, Cashflow: $3,000
• Go to Big Circle
• Get $300,000
"""

Scenario: I can rollback pay check transaction
	When Eoin Owens gets a paycheck
	But Eoin Owens rollbacks last action
	Then Eoin Owens has $300,010 in cash

Scenario: I can get money
	When Rory Hartman get $100,000 in cash
	Then Rory Hartman has $800,480 in cash
	And Rory Hartman history data is following:
"""
• Start a company. *Computer programs* - Price: $2,000
• *Computer programs* - Increase cashflow. $7,000
• Go to Big Circle
• Get $100,000
"""

Scenario: I can give money
	When Eoin Owens pays $100,000
	Then All users recieve notification: Eoin Owens transferred $100,000 to Bank.
	And Eoin Owens has $200,010 in cash
	And Eoin Owens history data is following:
"""
• Pay $500
• Buy Business. *Passage* - Price: $100,000, Mortgage: $99,000, Cashflow: $3,000
• Go to Big Circle
• Pay $100,000
"""

Scenario: I can see my friends
	When Rory Hartman says 'Friends'
	Then Rory Hartman can see friends:
"""
*On Small circle:*
• Genevieve Franklin
• Holly Bentley
• Deborah Case

*On Big circle:*
• Eoin Owens
"""

Scenario Outline: I can lost my money
	When Eoin Owens loses money because of <Trouble>
	Then Eoin Owens has <Amount> in cash
	And Eoin Owens last history record is: '• <Trouble>'

Examples:
	| Trouble   | Amount   |
	| Divorce   | $0       |
	| Tax Audit | $150,005 |
	| Lawsuit   | $150,005 |


Scenario: I can buy businesses
	When Eoin Owens gets a paycheck
	And Eoin Owens buys big businesses:
	| Title              | Price   | Cashflow |
	| Gold mine          | 100,000 |    3,000 |
	| Dry cleaning       | 150,000 |    5,000 |
	| Pizzeria franchise | 125,000 |    8,000 |
	| 60-plex            | 200,000 |   10,000 |
	Then Eoin Owens' details are following:
"""
*Profession:* Teacher
*Cash:* $25,010
Initial Cashflow: $300,000
Current Cashflow: $326,000
Target Cashflow: $350,000


*Assets:*
• *Gold mine* - Price: $100,000, Cashflow: $3,000
• *Dry cleaning* - Price: $150,000, Cashflow: $5,000
• *Pizzeria franchise* - Price: $125,000, Cashflow: $8,000
• *60-plex* - Price: $200,000, Cashflow: $10,000
"""
And Eoin Owens history data is following:
"""
• Pay $500
• Buy Business. *Passage* - Price: $100,000, Mortgage: $99,000, Cashflow: $3,000
• Go to Big Circle
• Get $300,000
• Buy Business. *Gold mine* - Price: $100,000, Cashflow: $3,000
• Buy Business. *Dry cleaning* - Price: $150,000, Cashflow: $5,000
• Buy Business. *Pizzeria franchise* - Price: $125,000, Cashflow: $8,000
• Buy Business. *60-plex* - Price: $200,000, Cashflow: $10,000
"""
