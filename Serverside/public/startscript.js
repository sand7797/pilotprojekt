function playerCount(code) {
  fetch("/playerCount", {
  method: "POST",
  body: JSON.stringify({ code: code }),
  headers: { "Content-Type": "application/json" }
})
  .then(res => res.json())
  .then(data => console.log(data))
  .catch(err => console.error(err));
}

function submit() {
  fetch('/gameCheck', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ code: document.getElementById("code").value })
  })
  .then(async res => {
    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.message || `Server responded with status: ${res.status}`);
    }
    return data;
  })
  .then(data => {
    console.log('Success:', data);
    window.location.href = "game.html";
  })
  .catch(err => {
    console.error('Error:', err.message);
    document.getElementById("errTxt").innerHTML = err.message
  });
}

const submitBtn = document.getElementById("submit");
submitBtn.addEventListener("click", submit);
