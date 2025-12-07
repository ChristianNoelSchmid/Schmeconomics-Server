import { AccountModel } from "@/api";

export function AccountCard(
    props: {
        account: AccountModel,
        onClick: (accountId: string) => void
    }
) {
    return (
        <div
            onClick={() => props.onClick(props.account.id)}
        >
            {props.account.name}
        </div>
    )
}