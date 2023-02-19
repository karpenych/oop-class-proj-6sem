using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    [SerializeField] AudioSource _as;
    [SerializeField] List<AudioClip> _songs;
    private List<AudioClip> _playedSongs = new();
    private float timer;
    private int songToPlay;



    void Start()
    {
        timer = 0;
        songToPlay = Random.Range(0, _songs.Count);
        _as.clip = _songs[songToPlay];
        _as.Play();
    }

    public void NextSongClick()
    {
        _playedSongs.Add(_songs[songToPlay]);
        _songs.RemoveAt(songToPlay);

        if (_songs.Count == 0)
        {
            var length = _playedSongs.Count;
            for (int i = 0; i < length; i++)
            {
                _songs.Add(_playedSongs[0]);
                _playedSongs.RemoveAt(0);
            }
        }

        songToPlay = Random.Range(0, _songs.Count);
        _as.clip = _songs[songToPlay];
        _as.Play();
        timer = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > _as.clip.length)
        {
            _playedSongs.Add(_songs[songToPlay]);
            _songs.RemoveAt(songToPlay);

            if (_songs.Count == 0)
            {
                var length = _playedSongs.Count;
                for (int i = 0; i < length; i++)
                {
                    _songs.Add(_playedSongs[0]);
                    _playedSongs.RemoveAt(0);
                }
            }

            songToPlay = Random.Range(0, _songs.Count);
            _as.clip = _songs[songToPlay];
            _as.Play();
            timer = 0;
        }
    }

}
