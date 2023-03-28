use Northwind
GO

CREATE TABLE [dbo].[OrgChart](
	[Hierarchy] [hierarchyid] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[Level] AS [Hierarchy].GetLevel() PERSISTED,
	CONSTRAINT [PK_OrgChart] PRIMARY KEY ([EmployeeId]),
	CONSTRAINT [FK_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeID])
);

SELECT e3.EmployeeID, e2.EmployeeID, e1.EmployeeID, e1.FirstName,e1.LastName
from Employees e1
LEFT JOIN Employees e2 on e1.ReportsTo= e2.EmployeeID 
LEFT JOIN Employees e3 on e2.ReportsTo = e3.EmployeeID
ORDER BY e1.EmployeeId;


WITH Children(ReportsTo, EmployeeID, Num)   
AS (
SELECT ReportsTo, EmployeeID, ROW_NUMBER() OVER (PARTITION BY ReportsTo ORDER BY ReportsTo) as Num
FROM Employees
),

paths(path, EmployeeID)   
AS (
-- This section provides the value for the root of the hierarchy
SELECT 
	hierarchyid::GetRoot() AS OrgNode, 
	EmployeeID
FROM Children AS C
WHERE ReportsTo IS NULL

UNION ALL
-- This section provides values for all nodes except the root
SELECT
	CAST(p.path.ToString() + CAST(C.ReportsTo AS varchar(30)) + '/' AS hierarchyid) AS OrgNode,
	C.EmployeeID
FROM Children AS C
JOIN paths AS p
   ON C.ReportsTo = P.EmployeeID
)
INSERT OrgChart(EmployeeID, Hierarchy)
SELECT O.EmployeeID, P.path
--SELECT O.EmployeeID, P.path.ToString(), O.FirstName, o.LastName, P.path.GetLevel()
FROM Employees AS O   
JOIN Paths AS P   
   ON O.EmployeeID = P.EmployeeID
order by o.EmployeeID
GO

SELECT Hierarchy.ToString(), EmployeeId, Level
FROM OrgChart
order by Hierarchy;

SELECT Hierarchy.ToString(), EmployeeId, Level
FROM OrgChart
WHERE hierarchyid::GetRoot() = Hierarchy
order by Hierarchy;


SELECT r.Hierarchy.ToString(), d.Hierarchy.ToString(), d.EmployeeId, d.Level
FROM OrgChart r
JOIN OrgChart d on d.Hierarchy.IsDescendantOf(r.Hierarchy) = 1 and d.Hierarchy.GetLevel() > r.Hierarchy.GetLevel()
WHERE r.EmployeeId = 2

