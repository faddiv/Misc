import { Card } from './Card';
import { createInitialDeck, Deck } from './Deck';
import { utils } from './utilities';

describe("createInitialDeck", () => {
  it("creates a Deck", () => {
    var deck = createInitialDeck();

    expect(deck).toBeDefined();
  });

  it("fills the Deck with 18 cards", () => {
    var deck = createInitialDeck();

    expect(deck.cards).toHaveLength(18);
  });

  it("initilazie the cards with values", () => {
    var deck = createInitialDeck();

    for (let index = 0; index < 9; index++) {
      expect(deck.cards[index * 2].value).toBe(index + 1);
      expect(deck.cards[index * 2 + 1].value).toBe(index + 1);
    }
  });

  it("to be connected to player", () => {
    var deck = createInitialDeck(1);

    for (let index = 0; index < deck.cards.length; index++) {
      const card = deck.cards[index];

      expect(card.player).toBe(1);
    }
  });

  it("to match to the snapshot", () => {
    var deck = createInitialDeck(2);

    expect(deck).toMatchSnapshot();
  });
});

describe("drawTopCard", () => {
  let deck: Deck;
  beforeEach(() => {
    deck = createInitialDeck(1);
  });

  it("returns a new deck", () => {
    const { newDeck } = deck.drawTopCard();

    expect(newDeck).toBeInstanceOf(Deck);
  });

  it("returns a card", () => {
    const { card } = deck.drawTopCard();

    expect(card).toBeInstanceOf(Card);
  });

  it("returns the first card from old deck", () => {
    const { card } = deck.drawTopCard();

    expect(card).toBe(deck.cards[0]);
  });

  it("removes first card from new deck", () => {
    const { newDeck, card } = deck.drawTopCard();

    expect(newDeck).not.toContain(card);
  });

  it("removes only one card from new deck", () => {
    const { newDeck } = deck.drawTopCard();

    expect(newDeck.cards).toHaveLength(deck.cards.length - 1);
  });

});

describe("shuffle", () => {
  let deck: Deck;

  beforeEach(() => {
    deck = createInitialDeck(1);
  });

  afterEach(() => {
    utils.random = Math.random;
  });

  it("returns a new deck", () => {
    const newDeck = deck.shuffle();

    expect(newDeck).toBeInstanceOf(Deck);
  });

  it("new deck contains the old cards", () => {
    const newDeck = deck.shuffle();

    deck.cards.forEach(card => {
      expect(newDeck.cards).toContain(card);
    });

  });

  it("new deck contains only the old cards", () => {
    const newDeck = deck.shuffle();

    expect(newDeck.cards).toHaveLength(deck.cards.length);

  });

  it("new deck contains the old cards shuffled", () => {
    let num = 0
    utils.random = () => {
      num += 457;
      return (num % 600) / 600;
    };
    const newDeck = deck.shuffle();

    expect(newDeck.cards).toMatchSnapshot();

  });

});


describe("pickCard", () => {
  let deck: Deck;
  beforeEach(() => {
    deck = createInitialDeck(1);
  });

  it("returns a new deck", () => {
    const { newDeck } = deck.pickCard(0);

    expect(newDeck).toBeInstanceOf(Deck);
  });

  it("returns a card", () => {
    const { card } = deck.pickCard(0);

    expect(card).toBeInstanceOf(Card);
  });

  it("returns the nth card from old deck", () => {
    const { card } = deck.pickCard(5);

    expect(card).toBe(deck.cards[5]);
  });

  it("removes the picked card from new deck", () => {
    const { newDeck, card } = deck.pickCard(5);

    expect(newDeck).not.toContain(card);
  });

  it("removes only one card from new deck", () => {
    const { newDeck } = deck.pickCard(5);

    expect(newDeck.cards).toHaveLength(deck.cards.length - 1);
  });

});
