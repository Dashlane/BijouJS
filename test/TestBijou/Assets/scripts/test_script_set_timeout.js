let counter = 0
setTimeout(() => {
    counter++
    sendToHost(counter.toString())
}, 100)