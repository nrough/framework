start "chess" /i /min /low ApproxReductBoostingCV.exe Data\chess.dta 20 100 1 | wtee chess.txt
start "semeion" /i /min /low ApproxReductBoostingCV.exe Data\semeion.data 20 100 1| wtee semeion.txt
start "zoo" /i /min /low ApproxReductBoostingCV.exe Data\zoo.data 20 100 1| wtee zoo.txt
