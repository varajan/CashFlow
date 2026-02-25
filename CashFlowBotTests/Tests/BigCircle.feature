@non-parallel
@do-cleanup

Feature: BigCircle

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
		#And Rory Hartman sells Computer Programs small business for $100,000
		#And Rory Hartman increases the cash flow of my small business by $7,000
		And Rory Hartman goes to the Big Circle

	When Eoin Owens buys businesses:
		| Title   | Price   | First Payment | Monthly Cashflow |
		| Passage | 100,000 |         1,000 |            3,000 |
		And Eoin Owens goes to the Big Circle

	But Deborah Case buys a boat


Scenario: I can get my pay check
	When Eoin Owens gets a paycheck
	Then Eoin Owens has $600,510 in cash
	And Eoin Owens history data is following:
"""
• Buy Business. *Passage* - Price: $100,000, Mortgage: $99,000, Cashflow: $3,000
• Go to Big Circle
• Get $300,000
"""

Scenario: I can rollback pay check transaction
	When Eoin Owens gets a paycheck
	But Eoin Owens rollbacks last action
	Then Eoin Owens has $300,510 in cash

Scenario: I can get money
	When Rory Hartman get $100,000 in cash
	Then Rory Hartman has $800,480 in cash
	And Rory Hartman history data is following:
"""
• Start a company. *Computer Programs* - Price: $2,000
• *Computer Programs* - Increase cash flow. $7,000
• Go to Big Circle
• Get $100,000
"""

Scenario: I can undo get money operation
	When Rory Hartman get $100,000 in cash
	But Rory Hartman rollbacks last action
	Then Rory Hartman has $700,480 in cash


Scenario: I can give money
	When Eoin Owens pays $100,000
	Then Eoin Owens has $200,510 in cash
	And Eoin Owens history data is following:
"""
• Buy Business. *Passage* - Price: $100,000, Mortgage: $99,000, Cashflow: $3,000
• Go to Big Circle
• Pay $100,000
"""

Scenario: I can rollback give money transaction
	When Eoin Owens pays $100,000
	But Eoin Owens rollbacks last action
	Then Eoin Owens has $300,510 in cash

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
	And Eoin Owens last history record is: • <Trouble>

Examples:
	| Trouble   | Amount   |
	| Divorce   | $0       |
	| Tax Audit | $150,255 |
	| Lawsuit   | $150,255 |

Scenario Outline: I can undo loose of money
	When Eoin Owens loses money because of <Trouble>
	But Eoin Owens rollbacks last action
	Then Eoin Owens has $300,510 in cash

Examples:
	| Trouble   |
	| Divorce   |
	| Tax Audit |
	| Lawsuit   |

# buy business
# win game

Scenario: I can stop the game
	When Rory Hartman decides to stop the game
		And Rory Hartman say 'Yes'
	Then The game is restarted

Scenario: I choose not to stop the game
	When Rory Hartman decides to stop the game
		And Rory Hartman say 'Cancel'
	Then The game is continued
