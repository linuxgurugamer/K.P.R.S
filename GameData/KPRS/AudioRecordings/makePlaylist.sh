#!/bin/bash
#

rm *.cfg

if [ $# -gt 0 ]; then
	dirs=$*
else
	dirs='*'
fi

MODDIR="KPRS/AudioRecordings"
for i in PluginData/$dirs; do
	if [ -d $i ]; then
	dirname=`echo $i | sed 's/PluginData\///'`
	name=`echo $dirname | sed 's/Channel//'`

	id=`echo $i | sed 's/[a-zA-Z\/]//g'`
	name=`echo $name | sed "s/$id//g"`

	channelCallSign="$MODDIR/PluginData/$dirname/$dirname"
	repeat=yes
	playlist=$dirname-playlist
	interplanetary=yes
	power=1
	cat >$i-station.cfg <<_EOF_
STATION
{
	name = $name
	channelNumber = $id
	abbr = $id
	channelCallSign = $channelCallSign
	repeat = $repeat
	repeatDelay = 5
	playlist = $playlist
	interplanetary = $interplanetary
	power = $power		// Transmission power
}
_EOF_

	cat >$playlist.cfg <<_EOF_
KPRS_PLAYLIST
{
	name = $playlist
	loop = True
	shuffle = True
	preloadTime = 5
	pauseOnGamePause = True
	disableAfterPlay = False
	TRACKS
	{
_EOF_
	for p in $i/*ogg; do
		if [ "$p" != "$i/*ogg" ]; then
			p1=`echo $p | sed 's/.ogg//g'`
			[ "$MODDIR/$p1" != "$channelCallSign" ] && echo "		track = ${MODDIR}/$p1" >> $playlist.cfg	
		fi
	done
	echo "	}" >> $playlist.cfg
	echo "}" >> $playlist.cfg

	fi
done

mv PluginData/*.cfg .
