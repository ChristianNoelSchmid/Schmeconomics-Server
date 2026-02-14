
export function currencyFormat(value: number) {
    const balance = Math.abs(value);
    const cents = (balance % 100);
    const dollars = Math.sign(value) * ((balance - cents) / 100);

    return `${dollars}.${cents.toString().padStart(2, '0')}`;
}