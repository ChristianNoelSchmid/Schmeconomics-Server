export enum CurrencyPartType {
    Plus,
    Minus,
}

export function stringToPartType(value: string) {
    switch(value) {
        case "+":
        case "Add": return CurrencyPartType.Plus;

        case "-":
        case "Subtract": return CurrencyPartType.Minus;
        default: throw Error(`Invalid value provided: "${value}"`);
    }
}

export function partsToValue(parts: CurrencyInputPart[]) {
    return parts.reduce<number>(
        (agg, cur) => agg + (cur.partType == CurrencyPartType.Plus ? cur.amount : -cur.amount),
        0
    );
}

export class CurrencyInputPart {
    constructor(partType: CurrencyPartType, amount: number) {
        this.partType = partType;
        this.amount = amount;
    }

    partType: CurrencyPartType = CurrencyPartType.Plus;
    amount: number = 0;
}