let counter = 0
const id = setInterval(() => {
    counter++
    sendToHost(counter.toString())
    clearInterval(id)
}, 100)