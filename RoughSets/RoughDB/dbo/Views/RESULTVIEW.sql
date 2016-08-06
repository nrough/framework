



CREATE VIEW [dbo].[RESULTVIEW]
AS
SELECT        D.DESCRIPTION AS EXPERIMENT, D.EXPERIMENTID AS EXPERMIENTID,  E.NAME AS DATASET, F.NAME AS IDENTIFICATION, F.NAMEALIAS AS IDENTIFICATIONALIAS, G.NAME AS VOTING, G.NAMEALIAS AS VOTINGALIAS, H.NAME AS WEIGHTING, 
                         H.NAMEALIAS AS WEIGHTINGALIAS, I.NAME AS MODEL, I.NAMEALIAS AS MODELALIAS, J.NAME AS EXCEPTION, J.NAMEALIAS AS EXCEPTIONALIAS, C.EPSILON, C.ENSEMBLESIZE, 
						 C.AVGACCURACY, C.MINACCURACY, C.MAXACCURACY, 
						 C.AVGBALANCEDACCURACY, C.MINBALANCEDACCURACY, C.MAXBALANCEDACCURACY, 
						 C.AVGCONFIDENCE, C.MINCONFIDENCE, C.MAXCONFIDENCE, 
						 C.AVGCOVERAGE, C.MINCOVERAGE, C.MAXCOVERAGE, 
						 C.AVGREDUCTLEN, C.MINREDUCTLEN, C.MAXREDUCTLEN, 
						 C.AVGEXCEPTIONRULEHITCOUNTER, C.AVGEXCEPTIONRULELENGTHSUM, C.AVGSTANDARDRULEHITCOUNTER, C.AVGSTANDARDRULELENGTHSUM, 
						 C.MINEXCEPTIONRULEHITCOUNTER, C.MINEXCEPTIONRULELENGTHSUM, C.MINSTANDARDRULEHITCOUNTER, C.MINSTANDARDRULELENGTHSUM, 
						 C.MAXEXCEPTIONRULEHITCOUNTER, C.MAXEXCEPTIONRULELENGTHSUM, C.MAXSTANDARDRULEHITCOUNTER, C.MAXSTANDARDRULELENGTHSUM,
						 C.AVGMODELCREATIONTIME, C.MINMODELCREATIONTIME, C.MAXMODELCREATIONTIME
FROM            (SELECT        EXPERIMENTID, DATASETID, MODELTYPEID, WEIGHTINGTYPEID, EXCEPTIONRULETYPEID, IDENTIFICATIONTYPEID, VOTINGTYPEID, ENSEMBLESIZE, EPSILON, AVG(ACCURACY) 
                                                    AS AVGACCURACY, AVG(BALANCEDACCURACY) AS AVGBALANCEDACCURACY, AVG(CONFIDENCE) AS AVGCONFIDENCE, AVG(COVERAGE) AS AVGCOVERAGE, AVG(REDUCTLEN) AS AVGREDUCTLEN, 
                                                    MIN(ACCURACY) AS MINACCURACY, MIN(BALANCEDACCURACY) AS MINBALANCEDACCURACY, MIN(CONFIDENCE) AS MINCONFIDENCE, MIN(COVERAGE) AS MINCOVERAGE, MIN(REDUCTLEN) 
                                                    AS MINREDUCTLEN, MAX(ACCURACY) AS MAXACCURACY, MAX(BALANCEDACCURACY) AS MAXBALANCEDACCURACY, MAX(CONFIDENCE) AS MAXCONFIDENCE, MAX(COVERAGE) AS MAXCOVERAGE, 
                                                    MAX(REDUCTLEN) AS MAXREDUCTLEN, AVG(EXCEPTIONRULEHITCOUNTER) AS AVGEXCEPTIONRULEHITCOUNTER, 
                                                    AVG(EXCEPTIONRULELENGTHSUM) AS AVGEXCEPTIONRULELENGTHSUM, AVG(STANDARDRULEHITCOUNTER) AS AVGSTANDARDRULEHITCOUNTER, AVG(STANDARDRULELENGTHSUM) 
                                                    AS AVGSTANDARDRULELENGTHSUM, MIN(EXCEPTIONRULEHITCOUNTER) AS MINEXCEPTIONRULEHITCOUNTER, MIN(EXCEPTIONRULELENGTHSUM) AS MINEXCEPTIONRULELENGTHSUM, 
                                                    MIN(STANDARDRULEHITCOUNTER) AS MINSTANDARDRULEHITCOUNTER, MIN(STANDARDRULELENGTHSUM) AS MINSTANDARDRULELENGTHSUM, MAX(EXCEPTIONRULEHITCOUNTER) 
                                                    AS MAXEXCEPTIONRULEHITCOUNTER, MAX(EXCEPTIONRULELENGTHSUM) AS MAXEXCEPTIONRULELENGTHSUM, MAX(STANDARDRULEHITCOUNTER) AS MAXSTANDARDRULEHITCOUNTER, 
                                                    MAX(STANDARDRULELENGTHSUM) AS MAXSTANDARDRULELENGTHSUM,
													AVG(MODELCREATIONTIME) AS AVGMODELCREATIONTIME, MIN(MODELCREATIONTIME) AS MINMODELCREATIONTIME, MAX(MODELCREATIONTIME) AS MAXMODELCREATIONTIME
                          FROM            (SELECT        EXPERIMENTID, TESTID, DATASETID, MODELTYPEID, WEIGHTINGTYPEID, EXCEPTIONRULETYPEID, IDENTIFICATIONTYPEID, VOTINGTYPEID, ENSEMBLESIZE, EPSILON, 
                                                                              AVG(ACCURACY) AS ACCURACY, AVG(BALANCEDACCURACY) AS BALANCEDACCURACY, AVG(CONFIDENCE) AS CONFIDENCE, AVG(COVERAGE) AS COVERAGE, 
                                                                              AVG(AVERAGEREDUCTLENGTH) AS REDUCTLEN, AVG(EXCEPTIONRULEHITCOUNTER) AS EXCEPTIONRULEHITCOUNTER, AVG(COALESCE(EXCEPTIONRULELENGTHSUM / NULLIF(EXCEPTIONRULEHITCOUNTER,0), 0)) 
                                                                              AS EXCEPTIONRULELENGTHSUM, AVG(STANDARDRULEHITCOUNTER) AS STANDARDRULEHITCOUNTER, AVG(COALESCE(STANDARDRULELENGTHSUM / NULLIF(STANDARDRULEHITCOUNTER,0), 0)) AS STANDARDRULELENGTHSUM,
																			  AVG(MODELCREATIONTIME) AS MODELCREATIONTIME
                                                    FROM            dbo.RESULT AS A
                                                    GROUP BY EXPERIMENTID, TESTID, DATASETID, MODELTYPEID, WEIGHTINGTYPEID, EPSILON, ENSEMBLESIZE, EXCEPTIONRULETYPEID, IDENTIFICATIONTYPEID, VOTINGTYPEID) AS B
                          GROUP BY EXPERIMENTID, DATASETID, MODELTYPEID, WEIGHTINGTYPEID, EPSILON, ENSEMBLESIZE, EXCEPTIONRULETYPEID, IDENTIFICATIONTYPEID, VOTINGTYPEID) AS C INNER JOIN
                         dbo.EXPERIMENT AS D ON D.EXPERIMENTID = C.EXPERIMENTID INNER JOIN
                         dbo.DATASET AS E ON E.DATASETID = C.DATASETID INNER JOIN
                         dbo.DECISIONRULEMEASURE AS F ON F.RULEQUALITYID = C.IDENTIFICATIONTYPEID INNER JOIN
                         dbo.DECISIONRULEMEASURE AS G ON G.RULEQUALITYID = C.VOTINGTYPEID INNER JOIN
                         dbo.WEIGHTINGTYPE AS H ON H.WEIGHTINGTYPEID = C.WEIGHTINGTYPEID INNER JOIN
                         dbo.MODELTYPE AS I ON I.MODELTYPEID = C.MODELTYPEID INNER JOIN
                         dbo.EXCEPTIONRULETYPE AS J ON J.EXCEPTIONTYPEID = C.EXCEPTIONRULETYPEID

	
	
	

GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'RESULTVIEW';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'd
         Begin Table = "C"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 315
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'RESULTVIEW';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[11] 2[30] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "D"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 234
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "E"
            Begin Extent = 
               Top = 138
               Left = 246
               Bottom = 234
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "F"
            Begin Extent = 
               Top = 234
               Left = 38
               Bottom = 347
               Right = 213
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "G"
            Begin Extent = 
               Top = 234
               Left = 251
               Bottom = 347
               Right = 426
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "H"
            Begin Extent = 
               Top = 348
               Left = 38
               Bottom = 461
               Right = 229
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "I"
            Begin Extent = 
               Top = 348
               Left = 267
               Bottom = 461
               Right = 437
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "J"
            Begin Extent = 
               Top = 462
               Left = 38
               Bottom = 575
               Right = 227
            End
            DisplayFlags = 280
            TopColumn = 0
         En', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'RESULTVIEW';

