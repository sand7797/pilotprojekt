const express = require('express');
const app = express();
const PORT = 8080;

require('dotenv').config();
app.use(express.json())
app.use(express.static('public'));

const session = require('express-session');
const FileStore = require('session-file-store')(session);

app.use(session({
  store: new FileStore({
    path: './sessions',
    ttl: 600,
    retries: 1
  }),
  secret: process.env.SESSION_SECRET,
  resave: false,
  saveUninitialized: false,
  cookie: { maxAge: 60 * 60 * 1000 } // 1H
}));

app.get('/checkTeam', (req, res) => {
  if (req.session.player < 5) {
    return res.json({player: req.session.player,team: 1});
  } else {
    return res.json({player: req.session.player,team: 2});
  }
});

app.get('/round', (req, res) => {
    const gameIndex = games.findIndex(item => item.code === req.session.game);
    return res.json({round: games[gameIndex].round, votes: games[gameIndex].votes});
});


app.post('/vote', (req, res) => {
    if (req.session.vote === undefined) req.session.vote = false;
    const gameIndex = games.findIndex(item => item.code === req.session.game);
    if(req.body.vote === true && req.session.vote === false) {
      req.session.vote = true;
      games[gameIndex].votes++;
    }
    return res.json({vote: games[gameIndex].votes});
    console.log(games);
});

const games = [];
games.push({code:"TEST", time:new Date(), players:0, round:0, actions:{}, votes: 0});
games.push({code:"TEST2", time:new Date(), players:0, round:0, actions:{}, votes: 0});

app.post('/gameCheck', (req, res) => {
  console.log('Received POST data:', req.body);
  code = req.body.code.toUpperCase();
  const exists = games.some(item => item.code === code);

  console.log(exists);
  //If games exits
  if (exists) {
    const gameIndex = games.findIndex(item => item.code === code);
    //If session doesnt exist gamecode
    if (req.session.game != games[gameIndex].code) {
      //If game < 8 players
      if (games[gameIndex].players <= 7) {
	games[gameIndex].players++;
	req.session.player = games[gameIndex].players
	req.session.game = games[gameIndex].code
	req.session.vote = false;
	//Sessions consists of player ID and code
	console.log(games[gameIndex], req.session.player, req.session.game);
	return res.json({ status: 'success', received: exists});
      //If game full
	} else {
	return res.status(410).json({ status: 'err', message: 'Spillet er startet uden dig, træls' });
      }
      //If sessions exists with same code
    } else if(req.session.player && req.session.game === games[gameIndex].code){
      res.json({ status: 'success', received: exists });
      console.log(games[gameIndex], req.session.player, req.session.game);
    } 
    //If game doesnt exist
  } else {
    return res.status(404).json({ status: 'err', message: '404: Spil ikke fundet, dobbeltjek spilkoden ' });
  }
});

//Ping for playercount
app.post('/playerCount', (req, res) => {
  if (req.body.code === "sessionID") {
    code = req.session.game
  } else {
    code = req.body.code.toUpperCase();
  }
  const gameIndex = games.findIndex(item => item.code === code);
  if (gameIndex === -1) {
    return res.status(404).json({ status: 'err', message: '404: Spil ikke fundet, dobbeltjek spilkoden ' });
  }
  return res.json({status: 'success', message: games[gameIndex].players})
});

app.post('/game', (req, res) => {
  games.push({code:req.body.code, time:new Date(), players:0, round:0, actions: {}, votes:0});
  console.log(games);
  res.json({ status: 'success', received: req.body });
});

app.post('/submit', (req, res) => {
  const code = req.session.game
  const gameIndex = games.findIndex(item => item.code === code);

  //Important vars
  const action = req.body.action
  const player = req.session.player
  
  console.log('action, player, gameindex:', action, player, games[gameIndex]);
  games[gameIndex].actions[player.toString()] = action;
  console.log(games[gameIndex]);

  res.json({ status: 'success', received: req.body });
});

app.post('/actions', (req, res) => {
  code = req.body.code.toUpperCase();
  const gameIndex = games.findIndex(item => item.code === code);
  res.json({ status: 'success', round: games[gameIndex].round, votes: games[gameIndex].votes, actions: games[gameIndex].actions });
  if (Object.keys(games[gameIndex].actions).length === 8) {
    games[gameIndex].actions = {}
    games[gameIndex].round++;
    console.log(games)
    return
  } else {
    return
  }
});

app.listen(PORT, '0.0.0.0',() => console.log('Server running on http://localhost:3000'));
