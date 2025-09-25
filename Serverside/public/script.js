let team;
async function sessionCheck(full) {
  try {
    const response = await fetch('checkTeam', {
      method: 'GET',
      credentials: 'include'
    });
    const data = await response.json();
    if (full === true) {
      document.getElementById("submit").classList.remove("hidden")
      if (data.team === 1) {
	team = 1;
	document.getElementById("team1").classList.remove("hidden")
      } else if (data.team === 2){
	team = 2;
	document.getElementById("team2").classList.remove("hidden")
      }
    }
    document.getElementById("holdtekst").innerHTML = "HOLD " + data.team.toString()
    document.getElementById("spillertekst").innerHTML = "Du er spiller " + data.player.toString()
    console.log(data.team)
    return data
  } catch (error) {
    console.error('Fetch error:', error);
    return false
  }
}

window.onload = async function() {
  console.log(sessionCheck(false))
};

const buttons = document.querySelectorAll('.item');
let active;

function vote() {
  checkRound();
  fetch('/vote', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({vote: true})
  })
  .then(res => res.json())
  .then(data => console.log(data));
  document.getElementById("kill").disabled = true;
}

const killBtn = document.getElementById("kill");
killBtn.addEventListener("click", vote);

const submitBtn = document.getElementById("submit");
submitBtn.addEventListener("click", submit);
submitBtn.disabled = true;

function submit() {
  console.log("submit")
  fetch('/submit', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({action: active})
  })
  .then(res => res.json())
  .then(data => console.log(data));
  submitBtn.disabled = true;
  if (team === 1) {
    document.getElementById("team1").classList.add("hidden")
  } else if (team === 2 ) {
    document.getElementById("team2").classList.add("hidden")
  }
}

buttons.forEach(button => {
  button.addEventListener('click', () => {
    active = button.id;
    button.classList.add('active')
    if (button.id !== "kill") {
      for (let i = 0; i < buttons.length; i++) {
	if (buttons[i].id !== active && buttons[i].id !== "kill") {
	  buttons[i].classList.remove('active');
	  submitBtn.disabled = false;
	}
	console.log(active)
      }
    }
  });
});

let checkEnabled = true;

function checkPlayers() {
  if (checkEnabled === true) {
    fetch('/playerCount', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code: "sessionID" })
    })
    .then(res => res.json())
    .then(data => {
      console.log("Player count:", data.message);

      if (data.message === 8) {
	document.getElementById("Startscreen").classList.add("hidden")
	sessionCheck(true);
	checkEnabled = false;
      }
    })
    .catch(err => console.error("Error fetching player count:", err));
  }
}

setInterval(checkPlayers, 500);


let round = 0;
let votes = 0;
function checkRound() {
  fetch("/round", {
  method: "GET",
  credentials: "include"
})
  .then(response => response.json())
  .then(data => {
    if(data.round > round) {
      submitBtn.disabled = false;
      if (team === 1) {
	document.getElementById("team1").classList.remove("hidden")
      } else if (team === 2 ) {
	document.getElementById("team2").classList.remove("hidden")
      }
      round = data.round
      console.log(data)
    }
    if (data.votes >= votes) {
      votes = data.votes
      document.getElementById("kill").innerHTML = "Dræb Åens Vogter <br> (" + votes +"/4 stemmer)"
    }
    if (votes === 4) {
      document.getElementById("fiskh1").disabled = false
      document.getElementById("fiskvogt").disabled = false
      document.getElementById("fiskh1").innerHTML = "Fisk <br> (+8 mad)"
      document.getElementById("fiskvogt").innerHTML = "Fisk og vogt åen <br> (+8 mad, stopper hold 1 i at fiske)"
    }
  });
}

setInterval(checkRound, 700);
