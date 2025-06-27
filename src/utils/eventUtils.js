const eventUtils = {
    // Message block
    // It is resposible for correct function of scrollbar
    waitForDOMUpdate: () => new Promise(resolve => requestAnimationFrame(() => setTimeout(resolve, 0)))
}

export default eventUtils;