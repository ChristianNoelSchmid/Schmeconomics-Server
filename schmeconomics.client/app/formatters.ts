
export function currencyFormat(value: number) {
    const balance = value;
    const cents = (balance % 100);
    const dollars = (balance - cents) / 100;

    return `${dollars}.${cents.toString().padStart(2, '0')}`;
}