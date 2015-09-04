start "soybean-large" /i /min /low ApproxReductBoosting.exe Data\soybean-large.data Data\soybean-large.test 20 100 1 1 1 1 | wtee soybean-large.txt
start "dna_modified" /i /min /low ApproxReductBoosting.exe Data\dna_modified.trn Data\dna_modified.tst 20 100 1 1 | wtee dna_modified.txt
start "letter" /i /min /low ApproxReductBoosting.exe Data\letter.trn Data\letter.tst 20 100 1 1 | wtee letter.txt
start "pen" /i /min /low ApproxReductBoosting.exe Data\pendigits.trn Data\pendigits.tst 20 100 1 1 | wtee pendigit.txt
start "opt" /i /min /low ApproxReductBoosting.exe Data\optdigits.trn Data\optdigits.tst 20 100 1 1 | wtee optdigit.txt
start "monks-1" /i /min /low ApproxReductBoosting.exe Data\monks-1.train Data\monks-1.test 20 100 1 1 | wtee monks-1.txt
start "monks-2" /i /min /low ApproxReductBoosting.exe Data\monks-2.train Data\monks-2.test 20 100 1 1 | wtee monks-2.txt
start "monks-3" /i /min /low ApproxReductBoosting.exe Data\monks-3.train Data\monks-3.test 20 100 1 1 | wtee monks-3.txt
start "spect" /i /min /low ApproxReductBoosting.exe Data\SPECT.trn Data\SPECT.tst 20 100 1 1 | wtee spect.txt
start "audiology.standarized.2" /i /min /low ApproxReductBoosting.exe Data\audiology.standardized.2.data Data\audiology.standardized.2.test 20 100 1 1 | wtee audiology.standarized.2.txt