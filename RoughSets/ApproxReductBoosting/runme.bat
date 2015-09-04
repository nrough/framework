start "letter" /i /min /low ApproxReductBoosting.exe Data\letter.trn Data\letter.tst 20 100 1| wtee letter.txt
start "monk-1" /i /min /low ApproxReductBoosting.exe Data\monks-1.train Data\monks-1.test 20 100 1| wtee monks-1.txt
start "monk-2" /i /min /low ApproxReductBoosting.exe Data\monks-2.train Data\monks-2.test 20 100 1| wtee monks-2.txt
start "monk-3" /i /min /low ApproxReductBoosting.exe Data\monks-3.train Data\monks-3.test 20 100 1| wtee monks-3.txt
start "dna_modified" /i /min /low ApproxReductBoosting.exe Data\dna_modified.trn Data\dna_modified.tst 20 100 1| wtee dna_modified.txt
start "pendigits" /i /min /low ApproxReductBoosting.exe Data\pendigits.trn Data\pendigits.tst 20 100 1| wtee pendigits.txt
start "optdigits" /i /min /low ApproxReductBoosting.exe Data\optdigits.trn Data\optdigits.tst 20 100 1| wtee optdigits.txt
start "spect" /i /min /low ApproxReductBoosting.exe Data\spect.train Data\spect.test 20 100 1| wtee spect.txt