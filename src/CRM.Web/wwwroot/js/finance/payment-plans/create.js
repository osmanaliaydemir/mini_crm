'use strict';

(function () {
    const planTypeSelect = document.getElementById('Input_PlanType');
    const amountInput = document.getElementById('Input_TotalAmount');
    const startDateInput = document.getElementById('Input_StartDate');
    const periodicityInput = document.getElementById('Input_PeriodicityWeeks');
    const container = document.getElementById('installments-container');
    const generateButton = document.getElementById('generateInstallments');

    if (!planTypeSelect || !amountInput || !startDateInput || !periodicityInput || !container || !generateButton) {
        return;
    }

    const renderInstallmentRow = (index, dueDate, amount) => {
        const wrapper = document.createElement('div');
        wrapper.className = 'installment-card';

        wrapper.innerHTML = `
            <div class=\"card shadow-sm\">
                <div class=\"card-body\">
                    <div class=\"installment-heading\">
                        <h6 class=\"mb-0\">#${index + 1}</h6>
                        <button type=\"button\" class=\"btn-close btn-remove\" aria-label=\"Remove\"></button>
                    </div>
                    <div class=\"form-grid\">
                        <div class=\"input-field\">
                            <label>Due Date</label>
                            <input name=\"Installments[${index}].DueDate\" class=\"input-control\" type=\"date\" value=\"${dueDate}\" />
                        </div>
                        <div class=\"input-field\">
                            <label>Amount</label>
                            <input name=\"Installments[${index}].Amount\" class=\"input-control\" value=\"${amount.toFixed(2)}\" />
                        </div>
                    </div>
                    <input type=\"hidden\" name=\"Installments[${index}].InstallmentNumber\" value=\"${index + 1}\" />
                </div>
            </div>`;

        const removeButton = wrapper.querySelector('.btn-remove');
        if (removeButton) {
            removeButton.addEventListener('click', () => {
                wrapper.remove();
                renumberInstallments();
            });
        }

        container.appendChild(wrapper);
    };

    const renumberInstallments = () => {
        const cards = container.querySelectorAll('.installment-card');
        cards.forEach((card, index) => {
            const heading = card.querySelector('h6');
            if (heading) {
                heading.textContent = `#${index + 1}`;
            }

            const dueDateInput = card.querySelector('input[name*=\"DueDate\"]');
            const amountInput = card.querySelector('input[name*=\"Amount\"]');
            const hiddenInput = card.querySelector('input[type=\"hidden\"]');

            if (dueDateInput) {
                dueDateInput.name = `Installments[${index}].DueDate`;
            }

            if (amountInput) {
                amountInput.name = `Installments[${index}].Amount`;
            }

            if (hiddenInput) {
                hiddenInput.name = `Installments[${index}].InstallmentNumber`;
                hiddenInput.value = index + 1;
            }
        });
    };

    generateButton.addEventListener('click', () => {
        container.innerHTML = '';

        if (planTypeSelect.value !== 'Installment') {
            return;
        }

        const total = parseFloat(amountInput.value || '0');
        const periodicity = parseInt(periodicityInput.value || '4', 10);
        const startDateValue = startDateInput.value || new Date().toISOString().substring(0, 10);
        const start = new Date(startDateValue);

        if (!Number.isFinite(total) || total <= 0) {
            return;
        }

        const installments = 4;
        const installmentAmount = total / installments;

        for (let i = 0; i < installments; i += 1) {
            const dueDate = new Date(start);
            dueDate.setDate(start.getDate() + i * periodicity * 7);
            renderInstallmentRow(i, dueDate.toISOString().substring(0, 10), installmentAmount);
        }
    });
})();

