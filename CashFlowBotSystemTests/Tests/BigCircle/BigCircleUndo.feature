Feature: Big Circle Undo

Background:
	Given Few players:
	| Name          | Profession |
	| Erin Richard  | Doctor     |
	| Gwen Walls    | Pilot      |
	| Henry Daniels | Lawyer     |
	| Mae Preston   | Teacher    |
	| Marley Ramsey | Secretary  |

	When Erin Richard gets 2 kids
	When Gwen Walls get $5,000 as a credit

	When Henry Daniels starts the Computer Programs company with $2000
		And Henry Daniels increases the cash flow of my small business by $7,000
		And Henry Daniels goes to the Big Circle

	When Mae Preston buys businesses:
		| Title   | Price   | First Payment | Monthly Cashflow |
		| Passage | 100,000 |         1,000 |            3,000 |
		And Mae Preston goes to the Big Circle

	But Marley Ramsey buys a boat


Scenario: I can undo get money operation
	When Henry Daniels get $100,000 in cash
	But Henry Daniels rollbacks last action
	Then Henry Daniels has $700,480 in cash

Scenario Outline: I can undo loose of money
	When Mae Preston loses money because of <Trouble>
	But Mae Preston rollbacks last action
	Then Mae Preston has $300,510 in cash

Examples:
	| Trouble   |
	| Divorce   |
	| Tax Audit |
	| Lawsuit   |


Scenario: I can rollback give money transaction
When Mae Preston pays $100,000
But Mae Preston rollbacks last action
Then Mae Preston has $300,510 in cash

Scenario: I can undo buy business
	When Mae Preston buys big businesses:
	| Title              | Price   | Cashflow |
	| Gold mine          | 100,000 |    3,000 |
	| 60-plex            | 200,000 |   10,000 |
	But Mae Preston rollbacks last action
	Then Mae Preston' details are following:
"""
*Profession:* Teacher
*Cash:* $200,510
Initial Cashflow: $300,000
Current Cashflow: $303,000
Target Cashflow: $350,000


*Assets:*
• *Gold mine* - Price: $100,000, Cashflow: $3,000
"""

Scenario: I can undo last actions after my victory
	When Henry Daniels buys big businesses:
	| Title              | Price   | Cashflow |
	| Gold mine          | 100,000 |   25,000 |
	| 60-plex            | 100,000 |   25,000 |
	But Henry Daniels rollbacks last action
	Then The game is continued for Henry Daniels
