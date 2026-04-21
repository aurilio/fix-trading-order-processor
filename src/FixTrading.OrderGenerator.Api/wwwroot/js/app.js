document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('orderForm');
    const result = document.getElementById('result');
    const submitBtn = document.getElementById('submitBtn');
    const statusDot = document.getElementById('statusDot');
    const statusText = document.getElementById('statusText');

    async function checkHealth() {
        try {
            const response = await fetch('/api/health');
            const data = await response.json();

            if (data.fixConnected) {
                statusDot.classList.add('connected');
                statusText.textContent = 'FIX Conectado';
            } else {
                statusDot.classList.remove('connected');
                statusText.textContent = 'FIX Desconectado';
            }
        } catch (error) {
            statusDot.classList.remove('connected');
            statusText.textContent = 'Erro de conexão';
        }
    }

    function showResult(isSuccess, content) {
        result.className = `result ${isSuccess ? 'success' : 'error'}`;
        result.innerHTML = content;
    }

    function buildOrderPayload() {
        return {
            symbol: document.getElementById('symbol').value,
            side: document.getElementById('side').value,
            quantity: parseInt(document.getElementById('quantity').value),
            price: parseFloat(document.getElementById('price').value)
        };
    }

    function formatSuccessMessage(data) {
        return `
            <h3>Ordem Enviada!</h3>
            <p><strong>ClOrdId:</strong> <span class="clordid">${data.clOrdId}</span></p>
            <p><strong>Status:</strong> ${data.status}</p>
            <p><strong>Mensagem:</strong> ${data.message}</p>
            <p><strong>Timestamp:</strong> ${new Date(data.timestamp).toLocaleString()}</p>
        `;
    }

    function formatErrorMessage(message) {
        return `
            <h3>Ordem Rejeitada</h3>
            <p><strong>Erro:</strong> ${message}</p>
        `;
    }

    async function submitOrder(order) {
        const response = await fetch('/api/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(order)
        });

        const data = await response.json();
        return { response, data };
    }

    async function handleSubmit(e) {
        e.preventDefault();

        submitBtn.disabled = true;
        submitBtn.textContent = 'Enviando...';
        result.className = 'result';
        result.style.display = 'none';

        try {
            const order = buildOrderPayload();
            const { response, data } = await submitOrder(order);

            if (response.ok && data.isSuccess) {
                showResult(true, formatSuccessMessage(data));
                form.reset();
            } else {
                const errorMsg = data.message || data.title || 'Erro desconhecido';
                showResult(false, formatErrorMessage(errorMsg));
            }
        } catch (error) {
            showResult(false, `<h3>Erro de Comunicação</h3><p>${error.message}</p>`);
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Enviar Ordem';
        }
    }

    // Inicialização
    checkHealth();
    setInterval(checkHealth, 5000);
    form.addEventListener('submit', handleSubmit);
});