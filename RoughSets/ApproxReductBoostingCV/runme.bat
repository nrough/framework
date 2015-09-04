start "chess" /i /min /low ApproxReductBoostingCV.exe Data\chess.data 20 100 1 1 | wtee chess.txt
start "semeion" /i /min /low ApproxReductBoostingCV.exe Data\semeion.data 20 100 1 1 | wtee semeion.txt
start "zoo" /i /min /low ApproxReductBoostingCV.exe Data\zoo.data 20 100 1 1 | wtee zoo.txt
start "soybean-small" /i /min /low ApproxReductBoostingCV.exe Data\soybean-small.2.data 20 100 1 1 | wtee soybean-small.txt
start "mashroom" /i /min /low ApproxReductBoostingCV.exe Data\agaricus-lepiota.2.data 20 100 1 1 | wtee mashroom.txt
start "promoters" /i /min /low ApproxReductBoostingCV.exe Data\promoters.2.data 20 100 1 1 | wtee promoters.txt
start "nursery" /i /min /low ApproxReductBoostingCV.exe Data\nursery.2.data 20 100 1 1 | wtee nursery.txt
start "house-votes-84" /i /min /low ApproxReductBoostingCV.exe Data\house-votes-84.2.data 20 100 1 1 3 1| wtee house-votes-84.txt
start "breast-cancer-wisconsin" /i /min /low ApproxReductBoostingCV.exe Data\breast-cancer-wisconsin.2.data 20 100 1 1 | wtee breast-cancer-wisconsin.txt
