// stripe-interop.js
// Loaded as an ES module via IJSObjectReference for isolation

let stripe = null;
let elements = null;
let cardElement = null;

export function initializeStripe(publishableKey) {
    try {
        if (!window.Stripe) {
            return 'Stripe.js not loaded. Add <script src="https://js.stripe.com/v3/"></script> to index.html';
        }
        stripe = window.Stripe(publishableKey);
        elements = stripe.elements();
        cardElement = elements.create('card', {
            style: {
                base: {
                    fontSize: '15px',
                    fontFamily: '"Inter", ui-sans-serif, system-ui, sans-serif',
                    color: '#18181b',
                    '::placeholder': { color: '#a1a1aa' },
                    iconColor: '#6366f1',
                },
                invalid: { color: '#ef4444', iconColor: '#ef4444' }
            }
        });
        cardElement.mount('#stripe-card-element');
        cardElement.on('change', (event) => {
            const errorEl = document.getElementById('stripe-card-errors');
            if (errorEl) {
                errorEl.textContent = event.error ? event.error.message : '';
            }
        });
        return null;
    } catch (e) {
        return e.message;
    }
}

export async function createPaymentMethod(cardholderName) {
    if (!stripe || !cardElement) return null;
    try {
        const { paymentMethod, error } = await stripe.createPaymentMethod({
            type: 'card',
            card: cardElement,
            billing_details: { name: cardholderName }
        });
        if (error) {
            const errorEl = document.getElementById('stripe-card-errors');
            if (errorEl) errorEl.textContent = error.message;
            return null;
        }
        return paymentMethod.id;
    } catch (e) {
        return null;
    }
}

export async function confirmCardPayment(clientSecret, cardholderName) {
    if (!stripe || !cardElement) return null;
    try {
        const { paymentIntent, error } = await stripe.confirmCardPayment(clientSecret, {
            payment_method: {
                card: cardElement,
                billing_details: { name: cardholderName }
            }
        });
        if (error) {
            const errorEl = document.getElementById('stripe-card-errors');
            if (errorEl) errorEl.textContent = error.message;
            return null;
        }
        return paymentIntent.id;
    } catch (e) {
        return null;
    }
}

export function destroyStripe() {
    if (cardElement) {
        cardElement.unmount();
        cardElement.destroy();
        cardElement = null;
    }
    elements = null;
    stripe = null;
}
