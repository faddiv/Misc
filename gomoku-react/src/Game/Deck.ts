import { Card } from "./Card";
import { utils } from "./utilities";

export class Deck {

  constructor(
    public readonly cards: Card[]) {

  }

  pickCard(index: number) {
    const cards = [...this.cards];
    const card = cards.splice(index, 1)[0];
    const newDeck = new Deck(cards);
    return {
      newDeck,
      card
    };
  }

  drawTopCard() {
    return this.pickCard(0);
  }

  shuffle() {
    const { cards } = this;
    const newCards = [...cards];
    for (let index = 0; index < newCards.length; index++) {
      const swapTarget = Math.floor(utils.random() * newCards.length);
      const card = newCards[index];
      const otherCard = newCards[swapTarget];
      newCards[index] = otherCard;
      newCards[swapTarget] = card;
    }
    return new Deck(newCards);
  }
}

export function createInitialDeck(player: number = 0) {
  const deck: Card[] = [];
  for (let index = 1; index < 10; index++) {
    deck.push(new Card(index, player));
    deck.push(new Card(index, player));
  }
  return new Deck(deck);
}
